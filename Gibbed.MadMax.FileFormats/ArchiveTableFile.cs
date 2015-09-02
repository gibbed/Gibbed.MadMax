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
using Gibbed.IO;

namespace Gibbed.MadMax.FileFormats
{
    public class ArchiveTableFile
    {
        public struct Entry
        {
            public readonly uint NameHash;
            public readonly uint Offset;
            public readonly uint CompressedSize;
            public readonly uint UncompressedSize;

            public Entry(
                uint nameHash,
                uint offset,
                uint compressedSize,
                uint uncompressedSize)
            {
                this.NameHash = nameHash;
                this.Offset = offset;
                this.CompressedSize = compressedSize;
                this.UncompressedSize = uncompressedSize;
            }
        }

        private Endian _Endian;
        private uint _Alignment;
        private readonly List<Entry> _Entries = new List<Entry>();

        public Endian Endian
        {
            get { return this._Endian; }
            set { this._Endian = value; }
        }

        public uint Alignment
        {
            get { return this._Alignment; }
            set { this._Alignment = value; }
        }

        public List<Entry> Entries
        {
            get { return this._Entries; }
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != 0x0800 && magic.Swap() != 0x0800)
            {
                throw new FormatException("strange alignment");
            }

            var endian = magic == 0x0800 ? Endian.Little : Endian.Big;
            var alignment = magic == 0x0800 ? magic : magic.Swap();
            var metadataCount = input.ReadValueU32(endian);

            // TODO: figure this out
            for (uint i = 0; i < metadataCount; i++)
            {
                var nameHash = input.ReadValueU32(endian);
                var valueCount = input.ReadValueU32(endian);

                for (uint j = 0; j < valueCount; j++)
                {
                    var offset = input.ReadValueU32(endian);
                    var subnameHash = input.ReadValueU32(endian);
                }
            }

            var entries = new List<Entry>();
            while (input.Position + 12 <= input.Length)
            {
                var nameHash = input.ReadValueU32(endian);
                var offset = input.ReadValueU32(endian);
                var compressedSize = input.ReadValueU32(endian);
                var uncompressedSize = input.ReadValueU32(endian);
                entries.Add(new Entry(nameHash, offset, compressedSize, uncompressedSize));
            }

            this._Endian = endian;
            this._Alignment = alignment;
            this._Entries.Clear();
            this._Entries.AddRange(entries);
        }
    }
}
