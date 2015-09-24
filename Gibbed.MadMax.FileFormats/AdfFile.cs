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
using System.Text;
using Gibbed.IO;

namespace Gibbed.MadMax.FileFormats
{
    public class AdfFile
    {
        public const uint Signature = 0x41444620; // 'ADF '

        #region Fields
        private Endian _Endian;
        private string _Comment;
        private readonly List<TypeDefinition> _TypeDefinitions;
        private readonly List<InstanceInfo> _InstanceInfos;
        #endregion

        public AdfFile()
        {
            this._TypeDefinitions = new List<TypeDefinition>();
            this._InstanceInfos = new List<InstanceInfo>();
        }

        #region Properties
        public Endian Endian
        {
            get { return this._Endian; }
            set { this._Endian = value; }
        }

        public string Comment
        {
            get { return this._Comment; }
            set { this._Comment = value; }
        }

        public List<TypeDefinition> TypeDefinitions
        {
            get { return this._TypeDefinitions; }
        }

        public List<InstanceInfo> InstanceInfos
        {
            get { return this._InstanceInfos; }
        }
        #endregion

        public void Serialize(Stream output)
        {
            throw new FormatException();
        }

        public void Deserialize(Stream input)
        {
            var basePosition = input.Position;

            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var version = input.ReadValueU32(endian);
            if (version != 4)
            {
                throw new FormatException();
            }

            var instanceCount = input.ReadValueU32(endian);
            var instanceOffset = input.ReadValueU32(endian);
            var typeDefinitionCount = input.ReadValueU32(endian);
            var typeDefinitionOffset = input.ReadValueU32(endian);
            var unknown18Count = input.ReadValueU32(endian);
            var unknown1COffset = input.ReadValueU32(endian);
            var nameTableCount = input.ReadValueU32(endian);
            var nameTableOffset = input.ReadValueU32(endian);
            var totalSize = input.ReadValueU32(endian);
            var unknown2C = input.ReadValueU32(endian);
            var unknown30 = input.ReadValueU32(endian);
            var unknown34 = input.ReadValueU32(endian);
            var unknown38 = input.ReadValueU32(endian);
            var unknown3C = input.ReadValueU32(endian);
            var comment = input.ReadStringZ(Encoding.ASCII);

            if (unknown18Count > 0 || unknown1COffset != 0)
            {
                throw new FormatException();
            }

            if (unknown2C != 0 || unknown30 != 0 || unknown34 != 0 || unknown38 != 0 || unknown3C != 0)
            {
                throw new FormatException();
            }

            if (basePosition + totalSize > input.Length)
            {
                throw new EndOfStreamException();
            }

            var names = new string[nameTableCount];
            if (nameTableCount > 0)
            {
                input.Position = basePosition + nameTableOffset;
                var nameLengths = new byte[nameTableCount];
                for (uint i = 0; i < nameTableCount; i++)
                {
                    nameLengths[i] = input.ReadValueU8();
                }
                for (uint i = 0; i < nameTableCount; i++)
                {
                    names[i] = input.ReadString(nameLengths[i], true, Encoding.ASCII);
                    input.Seek(1, SeekOrigin.Current);
                }
            }
            var stringTable = new StringTable(names);

            var typeDefinitions = new TypeDefinition[typeDefinitionCount];
            if (typeDefinitionCount > 0)
            {
                input.Position = basePosition + typeDefinitionOffset;
                for (uint i = 0; i < typeDefinitionCount; i++)
                {
                    typeDefinitions[i] = TypeDefinition.Read(input, endian, stringTable);
                }
            }

            var instanceInfos = new InstanceInfo[instanceCount];
            if (instanceCount > 0)
            {
                input.Position = basePosition + instanceOffset;
                for (uint i = 0; i < instanceCount; i++)
                {
                    instanceInfos[i] = InstanceInfo.Read(input, endian, stringTable);
                }
            }

            this._Endian = endian;
            this._Comment = comment;
            this._TypeDefinitions.Clear();
            this._TypeDefinitions.AddRange(typeDefinitions);
            this._InstanceInfos.Clear();
            this._InstanceInfos.AddRange(instanceInfos);
        }

        public enum TypeDefinitionType : uint
        {
            Primitive = 0,
            Structure = 1,
            Pointer = 2,
            Array = 3,
            InlineArray = 4,
            String = 5,
            BitField = 7,
            Enumeration = 8,
            StringHash = 9,
        }

        public struct TypeDefinition
        {
            public TypeDefinitionType Type;
            public uint Size;
            public uint Alignment;
            public uint NameHash;
            public string Name;
            public uint Flags;
            public uint ElementTypeHash;
            public uint ElementLength;
            public MemberDefinition[] Members;

            internal static TypeDefinition Read(Stream input, Endian endian, StringTable stringTable)
            {
                var instance = new TypeDefinition();
                instance.Type = (TypeDefinitionType)input.ReadValueU32(endian);
                instance.Size = input.ReadValueU32(endian);
                instance.Alignment = input.ReadValueU32(endian);
                instance.NameHash = input.ReadValueU32(endian);
                var nameIndex = input.ReadValueS64(endian);
                instance.Name = stringTable.Get(nameIndex);
                instance.Flags = input.ReadValueU32(endian);
                instance.ElementTypeHash = input.ReadValueU32(endian);
                instance.ElementLength = input.ReadValueU32(endian);

                switch (instance.Type)
                {
                    case TypeDefinitionType.Structure:
                    {
                        var memberCount = input.ReadValueU32(endian);
                        instance.Members = new MemberDefinition[memberCount];
                        for (uint i = 0; i < memberCount; i++)
                        {
                            instance.Members[i] = MemberDefinition.Read(input, endian, stringTable);
                        }
                        break;
                    }

                    case TypeDefinitionType.Array:
                    {
                        var memberCount = input.ReadValueU32(endian);
                        if (memberCount != 0)
                        {
                            throw new FormatException();
                        }
                        break;
                    }

                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                return instance;
            }

            internal void Write(Stream output, Endian endian)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return string.Format("{0}", this.Name);
            }
        }

        public struct MemberDefinition
        {
            public string Name;
            public uint TypeHash;
            public uint Size;
            public long Offset;
            public uint Unknown14;
            public uint Unknown18;

            internal static MemberDefinition Read(Stream input, Endian endian, StringTable stringTable)
            {
                var instance = new MemberDefinition();
                var nameIndex = input.ReadValueS64(endian);
                instance.Name = stringTable.Get(nameIndex);
                instance.TypeHash = input.ReadValueU32(endian);
                instance.Size = input.ReadValueU32(endian);
                instance.Offset = input.ReadValueS64(endian);
                instance.Unknown14 = input.ReadValueU32(endian);
                instance.Unknown18 = input.ReadValueU32(endian);
                return instance;
            }

            internal void Write(Stream output, Endian endian)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return string.Format("{0} ({1:X}) @ {2:X}", this.Name, this.TypeHash, this.Offset);
            }
        }

        public struct InstanceInfo
        {
            public uint NameHash;
            public uint TypeHash;
            public uint Offset;
            public uint Size;
            public string Name;

            internal static InstanceInfo Read(Stream input, Endian endian, StringTable stringTable)
            {
                var instance = new InstanceInfo();
                instance.NameHash = input.ReadValueU32(endian);
                instance.TypeHash = input.ReadValueU32(endian);
                instance.Offset = input.ReadValueU32(endian);
                instance.Size = input.ReadValueU32(endian);
                var nameIndex = input.ReadValueS64(endian);
                instance.Name = stringTable.Get(nameIndex);
                return instance;
            }

            internal void Write(Stream output, Endian endian)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return string.Format("{0} ({1:X})", this.Name, this.TypeHash);
            }
        }

        internal class StringTable
        {
            private readonly List<string> _Table;

            public StringTable(string[] names)
            {
                this._Table = names == null ? new List<string>() : new List<string>(names);
            }

            public string Get(long index)
            {
                if (index < 0 || index >= this._Table.Count || index > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return this._Table[(int)index];
            }

            public long Add(string text)
            {
                var index = this._Table.IndexOf(text);
                if (index >= 0)
                {
                    return index;
                }
                index = this._Table.Count;
                this._Table.Add(text);
                return index;
            }
        }
    }
}
