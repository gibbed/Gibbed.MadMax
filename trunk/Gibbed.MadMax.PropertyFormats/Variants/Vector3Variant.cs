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
using System.Globalization;
using System.IO;
using Gibbed.IO;

namespace Gibbed.MadMax.PropertyFormats.Variants
{
    public class Vector3Variant : IVariant, IRawVariant
    {
        public float X;
        public float Y;
        public float Z;

        public string Tag
        {
            get { return "vec"; }
        }

        public void Parse(string text)
        {
            var parts = text.Split(',');

            if (parts.Length != 3)
            {
                throw new FormatException("vec requires 3 float values delimited by commas");
            }

            this.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
            this.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
            this.Z = float.Parse(parts[2], CultureInfo.InvariantCulture);
        }

        public string Compose()
        {
            return string.Format("{0},{1},{2}",
                                 this.X.ToString(CultureInfo.InvariantCulture),
                                 this.Y.ToString(CultureInfo.InvariantCulture),
                                 this.Z.ToString(CultureInfo.InvariantCulture));
        }

        RawVariantType IRawVariant.Type
        {
            get { return RawVariantType.Vector3; }
        }

        void IRawVariant.Serialize(Stream output, Endian endian)
        {
            output.WriteValueF32(this.X, endian);
            output.WriteValueF32(this.Y, endian);
            output.WriteValueF32(this.Z, endian);
        }

        void IRawVariant.Deserialize(Stream input, Endian endian)
        {
            this.X = input.ReadValueF32(endian);
            this.Y = input.ReadValueF32(endian);
            this.Z = input.ReadValueF32(endian);
        }
    }
}
