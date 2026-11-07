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

namespace Gibbed.MadMax.ConvertSpreadsheet
{
    public class XlsBook
    {
        public const uint TypeHash = 0x0B73315D;

        #region Fields
        private readonly List<Worksheet> _Sheets;
        private readonly List<Cell> _Cells;
        private readonly List<string> _StringData;
        private readonly List<float> _ValueData;
        private readonly List<bool> _BoolData;
        private readonly List<string> _DateData;
        private readonly List<uint> _ColorData;
        private readonly List<CellAttribute> _Attributes;
        #endregion

        public XlsBook()
        {
            this._Sheets = new List<Worksheet>();
            this._Cells = new List<Cell>();
            this._StringData = new List<string>();
            this._ValueData = new List<float>();
            this._BoolData = new List<bool>();
            this._DateData = new List<string>();
            this._ColorData = new List<uint>();
            this._Attributes = new List<CellAttribute>();
        }

        #region Properties
        public List<Worksheet> Sheets
        {
            get { return this._Sheets; }
        }

        public List<Cell> Cells
        {
            get { return this._Cells; }
        }

        public List<string> StringData
        {
            get { return this._StringData; }
        }

        public List<float> ValueData
        {
            get { return this._ValueData; }
        }

        public List<bool> BoolData
        {
            get { return this._BoolData; }
        }

        public List<string> DateData
        {
            get { return this._DateData; }
        }

        public List<uint> ColorData
        {
            get { return this._ColorData; }
        }

        public List<CellAttribute> Attributes
        {
            get { return this._Attributes; }
        }
        #endregion

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            var basePosition = input.Position;

            var rawBook = RawBook.Read(input, endian);

            var sheets = new Worksheet[rawBook.SheetCount];
            if (rawBook.SheetCount != 0)
            {
                if (rawBook.SheetCount < 0 || rawBook.SheetCount > int.MaxValue)
                {
                    throw new FormatException();
                }

                var rawSheets = new RawWorksheet[rawBook.SheetCount];
                input.Position = basePosition + rawBook.SheetOffset;
                for (long i = 0; i < rawBook.SheetCount; i++)
                {
                    rawSheets[i] = RawWorksheet.Read(input, endian);
                }

                for (long i = 0; i < rawBook.SheetCount; i++)
                {
                    var rawSheet = rawSheets[i];

                    if (rawSheet.ColumnCount * rawSheet.RowCount != rawSheet.CellIndexCount)
                    {
                        throw new FormatException();
                    }

                    var sheet = new Worksheet();
                    sheet.ColumnCount = rawSheet.ColumnCount;
                    sheet.RowCount = rawSheet.RowCount;
                    sheet.CellIndices = new uint[rawSheet.CellIndexCount];

                    if (rawSheet.CellIndexCount != 0)
                    {
                        if (rawSheet.CellIndexCount < 0 || rawSheet.CellIndexCount > int.MaxValue)
                        {
                            throw new FormatException();
                        }

                        input.Position = basePosition + rawSheet.CellIndexOffset;
                        for (long j = 0; j < rawSheet.CellIndexCount; j++)
                        {
                            sheet.CellIndices[j] = input.ReadValueU32(endian);
                        }
                    }

                    if (rawSheet.NameOffset == 0)
                    {
                        sheet.Name = null;
                    }
                    else
                    {
                        input.Position = basePosition + rawSheet.NameOffset;
                        sheet.Name = input.ReadStringZ(Encoding.ASCII);
                    }

                    sheets[i] = sheet;
                }
            }

            var cells = new Cell[rawBook.CellCount];
            if (rawBook.CellCount != 0)
            {
                if (rawBook.CellCount < 0 || rawBook.CellCount > int.MaxValue)
                {
                    throw new FormatException();
                }

                input.Position = basePosition + rawBook.CellOffset;
                for (long i = 0; i < rawBook.CellCount; i++)
                {
                    cells[i] = Cell.Read(input, endian);
                }
            }

            var stringData = new string[rawBook.StringDataCount];
            if (rawBook.StringDataCount != 0)
            {
                if (rawBook.StringDataCount < 0 || rawBook.StringDataCount > int.MaxValue)
                {
                    throw new FormatException();
                }

                input.Position = basePosition + rawBook.StringDataOffset;
                var rawStringData = new long[rawBook.StringDataCount];
                for (long i = 0; i < rawBook.StringDataCount; i++)
                {
                    rawStringData[i] = input.ReadValueS64(endian);
                }
                for (long i = 0; i < rawBook.StringDataCount; i++)
                {
                    input.Position = basePosition + rawStringData[i];
                    stringData[i] = input.ReadStringZ(Encoding.ASCII);
                }
            }

            var valueData = new float[rawBook.ValueDataCount];
            if (rawBook.ValueDataCount != 0)
            {
                if (rawBook.ValueDataCount < 0 || rawBook.ValueDataCount > int.MaxValue)
                {
                    throw new FormatException();
                }

                input.Position = basePosition + rawBook.ValueDataOffset;
                for (long i = 0; i < rawBook.ValueDataCount; i++)
                {
                    valueData[i] = input.ReadValueF32(endian);
                }
            }

            var boolData = new bool[rawBook.BoolDataCount];
            if (rawBook.BoolDataCount != 0)
            {
                if (rawBook.BoolDataCount < 0 || rawBook.BoolDataCount > int.MaxValue)
                {
                    throw new FormatException();
                }

                input.Position = basePosition + rawBook.BoolDataOffset;
                for (long i = 0; i < rawBook.BoolDataCount; i++)
                {
                    boolData[i] = input.ReadValueB8();
                }
            }

            var dateData = new string[rawBook.DateDataCount];
            if (rawBook.DateDataCount != 0)
            {
                if (rawBook.DateDataCount < 0 || rawBook.DateDataCount > int.MaxValue)
                {
                    throw new FormatException();
                }

                input.Position = basePosition + rawBook.DateDataOffset;
                var rawDateData = new long[rawBook.DateDataCount];
                for (long i = 0; i < rawBook.DateDataCount; i++)
                {
                    rawDateData[i] = input.ReadValueS64(endian);
                }
                for (long i = 0; i < rawBook.DateDataCount; i++)
                {
                    input.Position = basePosition + rawDateData[i];
                    dateData[i] = input.ReadStringZ(Encoding.ASCII);
                }
            }

            var colorData = new uint[rawBook.ColorDataCount];
            if (rawBook.ColorDataCount != 0)
            {
                if (rawBook.ColorDataCount < 0 || rawBook.ColorDataCount > int.MaxValue)
                {
                    throw new FormatException();
                }

                input.Position = basePosition + rawBook.ColorDataOffset;
                for (long i = 0; i < rawBook.ColorDataCount; i++)
                {
                    colorData[i] = input.ReadValueU32(endian);
                }
            }

            var attributes = new CellAttribute[rawBook.AttributeCount];
            if (rawBook.AttributeCount != 0)
            {
                if (rawBook.AttributeCount < 0 || rawBook.AttributeCount > int.MaxValue)
                {
                    throw new FormatException();
                }

                input.Position = basePosition + rawBook.AttributeOffset;
                for (long i = 0; i < rawBook.AttributeCount; i++)
                {
                    attributes[i] = CellAttribute.Read(input, endian);
                }
            }

            this._Sheets.Clear();
            this._Sheets.AddRange(sheets);
            this._Cells.Clear();
            this._Cells.AddRange(cells);
            this._StringData.Clear();
            this._StringData.AddRange(stringData);
            this._ValueData.Clear();
            this._ValueData.AddRange(valueData);
            this._BoolData.Clear();
            this._BoolData.AddRange(boolData);
            this._DateData.Clear();
            this._DateData.AddRange(dateData);
            this._ColorData.Clear();
            this._ColorData.AddRange(colorData);
            this._Attributes.Clear();
            this._Attributes.AddRange(attributes);
        }

        private struct RawBook
        {
            public long SheetOffset;
            public long SheetCount;
            public long CellOffset;
            public long CellCount;
            public long StringDataOffset;
            public long StringDataCount;
            public long ValueDataOffset;
            public long ValueDataCount;
            public long BoolDataOffset;
            public long BoolDataCount;
            public long DateDataOffset;
            public long DateDataCount;
            public long ColorDataOffset;
            public long ColorDataCount;
            public long AttributeOffset;
            public long AttributeCount;

            public static RawBook Read(Stream input, Endian endian)
            {
                var instance = new RawBook();
                instance.SheetOffset = input.ReadValueS64(endian);
                instance.SheetCount = input.ReadValueS64(endian);
                instance.CellOffset = input.ReadValueS64(endian);
                instance.CellCount = input.ReadValueS64(endian);
                instance.StringDataOffset = input.ReadValueS64(endian);
                instance.StringDataCount = input.ReadValueS64(endian);
                instance.ValueDataOffset = input.ReadValueS64(endian);
                instance.ValueDataCount = input.ReadValueS64(endian);
                instance.BoolDataOffset = input.ReadValueS64(endian);
                instance.BoolDataCount = input.ReadValueS64(endian);
                instance.DateDataOffset = input.ReadValueS64(endian);
                instance.DateDataCount = input.ReadValueS64(endian);
                instance.ColorDataOffset = input.ReadValueS64(endian);
                instance.ColorDataCount = input.ReadValueS64(endian);
                instance.AttributeOffset = input.ReadValueS64(endian);
                instance.AttributeCount = input.ReadValueS64(endian);
                return instance;
            }
        }

        public struct Worksheet
        {
            public uint ColumnCount;
            public uint RowCount;
            public uint[] CellIndices;
            public string Name;
        }

        private struct RawWorksheet
        {
            public uint ColumnCount;
            public uint RowCount;
            public long CellIndexOffset;
            public long CellIndexCount;
            public long NameOffset;

            public static RawWorksheet Read(Stream input, Endian endian)
            {
                var instance = new RawWorksheet();
                instance.ColumnCount = input.ReadValueU32(endian);
                instance.RowCount = input.ReadValueU32(endian);
                instance.CellIndexOffset = input.ReadValueS64(endian);
                instance.CellIndexCount = input.ReadValueS64(endian);
                instance.NameOffset = input.ReadValueS64(endian);
                return instance;
            }
        }

        public enum CellType : uint
        {
            Bool = 0,
            String = 1,
            Float = 2,
        }

        public struct Cell
        {
            public CellType Type;
            public uint DataIndex;
            public uint AttributeIndex;

            internal static Cell Read(Stream input, Endian endian)
            {
                var instance = new Cell();
                instance.Type = (CellType)input.ReadValueU32(endian);
                instance.DataIndex = input.ReadValueU32(endian);
                instance.AttributeIndex = input.ReadValueU32(endian);
                return instance;
            }
        }

        public struct CellAttribute
        {
            public byte ForegroundColorIndex;
            public byte BackgroundColorIndex;

            internal static CellAttribute Read(Stream input, Endian endian)
            {
                var instance = new CellAttribute();
                instance.ForegroundColorIndex = input.ReadValueU8();
                instance.BackgroundColorIndex = input.ReadValueU8();
                return instance;
            }
        }
    }
}
