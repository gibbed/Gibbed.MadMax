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
        private Endian _Endian;
        private uint _Alignment;
        private readonly Dictionary<uint, EntryInfo> _Entries;
        private readonly Dictionary<uint, IList<EntryChunkInfo>> _EntryChunks;

        public ArchiveTableFile()
        {
            this._Entries = new Dictionary<uint, EntryInfo>();
            this._EntryChunks = new Dictionary<uint, IList<EntryChunkInfo>>();
        }

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

        public Dictionary<uint, EntryInfo> Entries
        {
            get { return this._Entries; }
        }

        public Dictionary<uint, IList<EntryChunkInfo>> EntryChunks
        {
            get { return this._EntryChunks; }
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

            var entryChunkListCount = input.ReadValueU32(endian);
            var entryChunks = new Dictionary<uint, EntryChunkInfo[]>();
            for (uint i = 0; i < entryChunkListCount; i++)
            {
                var nameHash = input.ReadValueU32(endian);
                var chunkCount = input.ReadValueU32(endian);
                var chunks = new EntryChunkInfo[chunkCount];
                for (uint j = 0; j < chunkCount; j++)
                {
                    var uncompressedOffset = input.ReadValueU32(endian);
                    var compressedOffset = input.ReadValueU32(endian);
                    chunks[j] = new EntryChunkInfo(uncompressedOffset, compressedOffset);
                }
                entryChunks.Add(nameHash, chunks);
            }

            var entries = new Dictionary<uint, EntryInfo>();
            while (input.Position + 12 <= input.Length)
            {
                var nameHash = input.ReadValueU32(endian);
                var offset = input.ReadValueU32(endian);
                var compressedSize = input.ReadValueU32(endian);
                var uncompressedSize = input.ReadValueU32(endian);
                entries.Add(nameHash, new EntryInfo(offset, compressedSize, uncompressedSize));
            }

            this._Endian = endian;
            this._Alignment = alignment;

            this._Entries.Clear();
            foreach (var kv in entries)
            {
                this._Entries.Add(kv.Key, kv.Value);
            }

            this._EntryChunks.Clear();
            foreach (var kv in entryChunks)
            {
                this._EntryChunks.Add(kv.Key, Array.AsReadOnly(kv.Value));
            }
        }

        public struct EntryInfo
        {
            public readonly uint Offset;
            public readonly uint CompressedSize;
            public readonly uint UncompressedSize;

            public EntryInfo(uint offset, uint compressedSize, uint uncompressedSize)
            {
                this.Offset = offset;
                this.CompressedSize = compressedSize;
                this.UncompressedSize = uncompressedSize;
            }
        }

        public struct EntryChunkInfo
        {
            public readonly uint UncompressedOffset;
            public readonly uint CompressedOffset;

            public EntryChunkInfo(uint uncompressedOffset, uint compressedOffset)
            {
                this.UncompressedOffset = uncompressedOffset;
                this.CompressedOffset = compressedOffset;
            }
        }
    }
}
