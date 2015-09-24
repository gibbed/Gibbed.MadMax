/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using NDesk.Options;
using XvmOpcode = Gibbed.MadMax.FileFormats.XvmOpcode;

namespace Gibbed.MadMax.XvmDisassemble
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private static void Main(string[] args)
        {
            bool showHelp = false;

            var options = new OptionSet
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 ||
                showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_xvmc [output_dis]", GetExecutableName());
                //Console.WriteLine("Convert an ADF file between binary and XML format.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, ".dis");

            Endian endian;
            var adf = new FileFormats.AdfFile();
            var module = new FileFormats.XvmModule();
            MemoryStream debugStrings = null;

            using (var input = File.OpenRead(inputPath))
            {
                adf.Deserialize(input);
                endian = adf.Endian;

                if (adf.TypeDefinitions.Count > 0)
                {
                    throw new NotSupportedException();
                }

                var debugStringsInfo = adf.InstanceInfos.FirstOrDefault(i => i.Name == "debug_strings");
                if (debugStringsInfo.TypeHash == 0xFEF3B589)
                {
                    input.Position = debugStringsInfo.Offset;
                    using (var data = input.ReadToMemoryStream(debugStringsInfo.Size))
                    {
                        var offset = data.ReadValueS64(endian);
                        var count = data.ReadValueS64(endian);
                        if (count < 0 || count > int.MaxValue)
                        {
                            throw new FormatException();
                        }

                        data.Position = offset;
                        debugStrings = new MemoryStream(data.ReadBytes((int)count), false);
                    }
                }

                var moduleInfo = adf.InstanceInfos.First(i => i.Name == "module");
                if (moduleInfo.TypeHash != FileFormats.XvmModule.TypeHash)
                {
                    throw new FormatException();
                }

                input.Position = moduleInfo.Offset;
                using (var data = input.ReadToMemoryStream(moduleInfo.Size))
                {
                    module.Deserialize(data, endian);
                }
            }

            using (var output = File.Create(outputPath))
            using (var streamWriter = new StreamWriter(output))
            using (var writer = new System.CodeDom.Compiler.IndentedTextWriter(streamWriter))
            using (debugStrings)
            {
                foreach (var function in module.Functions)
                {
                    writer.WriteLine();
                    writer.WriteLine("== {0} ==", function.Name);

                    var labels = new string[function.Instructions.Length];
                    for (int i = 0; i < function.Instructions.Length; i++)
                    {
                        var instruction = function.Instructions[i];
                        var opcode = (XvmOpcode)(instruction & 0x1F);
                        var oparg = instruction >> 5;

                        if (opcode == XvmOpcode.Jmp ||
                            opcode == XvmOpcode.Jz)
                        {
                            if (labels[oparg] == null)
                            {
                                labels[oparg] = string.Format("label_{0}", oparg);
                            }
                        }
                    }

                    writer.Indent++;

                    for (int i = 0; i < function.Instructions.Length; i++)
                    {
                        if (labels[i] != null)
                        {
                            writer.Indent--;
                            writer.WriteLine("{0}:", labels[i]);
                            writer.Indent++;
                        }

                        var instruction = function.Instructions[i];
                        var opcode = (XvmOpcode)(instruction & 0x1F);
                        var oparg = instruction >> 5;

                        if (_SimpleStatements.ContainsKey(opcode) == true)
                        {
                            writer.Write("{0}", _SimpleStatements[opcode]);
                        }
                        else
                        {
                            switch (opcode)
                            {
                                case XvmOpcode.Call:
                                {
                                    writer.Write("call {0}", oparg);
                                    break;
                                }

                                case XvmOpcode.Jmp:
                                {
                                    writer.Write("jmp {0}", labels[oparg]);
                                    break;
                                }

                                case XvmOpcode.Jz:
                                {
                                    writer.Write("jz {0}", labels[oparg]);
                                    break;
                                }

                                case XvmOpcode.LoadAttr:
                                {
                                    writer.Write("loadattr ");
                                    var constant = module.Constants[oparg];

                                    if (constant.Type != 4)
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    if (debugStrings != null)
                                    {
                                        var debugStringOffset = (module.StringBuffer[constant.Value - 2] << 8) |
                                                                (module.StringBuffer[constant.Value - 1] << 0);
                                        debugStrings.Position = debugStringOffset;
                                        var text = debugStrings.ReadStringZ(Encoding.UTF8);
                                        writer.Write("\"{0}\"", Escape(text));
                                    }
                                    else
                                    {
                                        var hashIndex = module.StringBuffer[constant.Value - 3];
                                        var hash = module.StringHashes[hashIndex];
                                        writer.Write("0x{0:X}", hash);
                                    }

                                    break;
                                }

                                case XvmOpcode.LoadConst:
                                {
                                    writer.Write("loadconst ");
                                    var constant = module.Constants[oparg];

                                    writer.Write("{0} // FIXME", oparg);

                                    break;
                                }

                                case XvmOpcode.LoadGlobal:
                                {
                                    writer.Write("loadglobal ");
                                    var constant = module.Constants[oparg];

                                    if (constant.Type != 4)
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    if (debugStrings != null)
                                    {
                                        var debugStringOffset = (module.StringBuffer[constant.Value - 2] << 8) |
                                                                (module.StringBuffer[constant.Value - 1] << 0);
                                        debugStrings.Position = debugStringOffset;
                                        var text = debugStrings.ReadStringZ(Encoding.UTF8);
                                        writer.Write("\"{0}\"", Escape(text));
                                    }
                                    else
                                    {
                                        var hashIndex = module.StringBuffer[constant.Value - 3];
                                        var hash = module.StringHashes[hashIndex];
                                        writer.Write("0x{0:X}", hash);
                                    }

                                    break;
                                }

                                case XvmOpcode.LoadLocal:
                                {
                                    writer.Write("loadlocal {0}", oparg);
                                    break;
                                }

                                case XvmOpcode.Ret:
                                {
                                    writer.Write("ret {0}", oparg);
                                    break;
                                }

                                case XvmOpcode.StoreAttr:
                                {
                                    writer.Write("storeattr ");
                                    var constant = module.Constants[oparg];

                                    if (constant.Type != 4)
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    if (debugStrings != null)
                                    {
                                        var debugStringOffset = (module.StringBuffer[constant.Value - 2] << 8) |
                                                                (module.StringBuffer[constant.Value - 1] << 0);
                                        debugStrings.Position = debugStringOffset;
                                        var text = debugStrings.ReadStringZ(Encoding.UTF8);
                                        writer.Write("\"{0}\"", Escape(text));
                                    }
                                    else
                                    {
                                        var hashIndex = module.StringBuffer[constant.Value - 3];
                                        var hash = module.StringHashes[hashIndex];
                                        writer.Write("0x{0:X}", hash);
                                    }

                                    break;
                                }

                                case XvmOpcode.StoreLocal:
                                {
                                    writer.Write("storelocal {0}", oparg);
                                    break;
                                }

                                default:
                                {
                                    throw new NotSupportedException();
                                }
                            }
                        }

                        writer.WriteLine();
                    }

                    writer.Indent--;
                }
            }
        }

        private static string Escape(string input)
        {
            var sb = new StringBuilder();
            foreach (char t in input)
            {
                switch (t)
                {
                    case '"':
                    {
                        sb.Append("\\\"");
                        break;
                    }

                    case '\t':
                    {
                        sb.Append("\\t");
                        break;
                    }
                    case '\r':
                    {
                        sb.Append("\\r");
                        break;
                    }
                    case '\n':
                    {
                        sb.Append("\\n");
                        break;
                    }

                    default:
                    {
                        sb.Append(t);
                        break;
                    }
                }
            }
            return sb.ToString();
        }

        private static readonly Dictionary<XvmOpcode, string> _SimpleStatements =
            new Dictionary<XvmOpcode, string>()
            {
                { XvmOpcode.Assert, "assert" },
                { XvmOpcode.And, "and" },
                { XvmOpcode.Or, "or" },
                { XvmOpcode.Add, "add" },
                { XvmOpcode.Div, "div" },
                { XvmOpcode.Mod, "mod" },
                { XvmOpcode.Mul, "mul" },
                { XvmOpcode.Sub, "sub" },
                { XvmOpcode.CmpEq, "cmpeq" },
                { XvmOpcode.CmpGe, "cmpge" },
                { XvmOpcode.CmpG, "cmpg" },
                { XvmOpcode.CmpNe, "cmpne" },
                { XvmOpcode.LoadItem, "loaditem" },
                { XvmOpcode.Pop, "pop" },
                { XvmOpcode.StoreItem, "storeitem" },
                { XvmOpcode.IsZero, "iszero" },
                { XvmOpcode.Neg, "neg" },
            };
    }
}
