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
using Gibbed.IO;
using NDesk.Options;

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
            var typeLibraryPaths = new List<string>();

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

            using (var input = File.OpenRead(inputPath))
            {
                adf.Deserialize(input);
                endian = adf.Endian;

                if (adf.TypeDefinitions.Count > 0)
                {
                    throw new NotSupportedException();
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
            using (var writer = new StreamWriter(output))
            {
                foreach (var function in module.Functions)
                {
                    writer.WriteLine("{0}:", function.Name);
                    foreach (var instruction in function.Instructions)
                    {
                        var opcode = (FileFormats.XvmOpcode)(instruction & 0x1F);
                        var oparg = instruction >> 5;
                        writer.WriteLine("  {0:X4} {1} {2}", instruction, opcode, oparg);
                    }
                }
            }
        }
    }
}
