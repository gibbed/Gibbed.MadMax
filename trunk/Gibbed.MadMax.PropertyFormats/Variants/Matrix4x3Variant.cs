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
    // ReSharper disable InconsistentNaming
    public class Matrix4x3Variant : IVariant, IRawVariant
        // ReSharper restore InconsistentNaming
    {
        public float M11;
        public float M12;
        public float M13;

        public float M21;
        public float M22;
        public float M23;

        public float M31;
        public float M32;
        public float M33;

        public float M41;
        public float M42;
        public float M43;

        public string Tag
        {
            get { return "mat"; }
        }

        public void Parse(string text)
        {
            var parts = text.Split(',');
            if (parts.Length != 3 * 4)
            {
                throw new FormatException("mat requires 12 float values delimited by commas");
            }

            this.M11 = float.Parse(parts[0], CultureInfo.InvariantCulture);
            this.M12 = float.Parse(parts[1], CultureInfo.InvariantCulture);
            this.M13 = float.Parse(parts[2], CultureInfo.InvariantCulture);
            this.M21 = float.Parse(parts[3], CultureInfo.InvariantCulture);
            this.M22 = float.Parse(parts[4], CultureInfo.InvariantCulture);
            this.M23 = float.Parse(parts[5], CultureInfo.InvariantCulture);
            this.M31 = float.Parse(parts[6], CultureInfo.InvariantCulture);
            this.M32 = float.Parse(parts[7], CultureInfo.InvariantCulture);
            this.M33 = float.Parse(parts[8], CultureInfo.InvariantCulture);
            this.M41 = float.Parse(parts[9], CultureInfo.InvariantCulture);
            this.M42 = float.Parse(parts[10], CultureInfo.InvariantCulture);
            this.M43 = float.Parse(parts[11], CultureInfo.InvariantCulture);
        }

        public string Compose()
        {
            return String.Format(
                "{0},{1},{2}, {3},{4},{5}, {6},{7},{8}, {9},{10},{11}",
                this.M11.ToString(CultureInfo.InvariantCulture),
                this.M12.ToString(CultureInfo.InvariantCulture),
                this.M13.ToString(CultureInfo.InvariantCulture),
                this.M21.ToString(CultureInfo.InvariantCulture),
                this.M22.ToString(CultureInfo.InvariantCulture),
                this.M23.ToString(CultureInfo.InvariantCulture),
                this.M31.ToString(CultureInfo.InvariantCulture),
                this.M32.ToString(CultureInfo.InvariantCulture),
                this.M33.ToString(CultureInfo.InvariantCulture),
                this.M41.ToString(CultureInfo.InvariantCulture),
                this.M42.ToString(CultureInfo.InvariantCulture),
                this.M43.ToString(CultureInfo.InvariantCulture));
        }

        RawVariantType IRawVariant.Type
        {
            get { return RawVariantType.Matrix4x3; }
        }

        void IRawVariant.Serialize(Stream output, Endian endian)
        {
            output.WriteValueF32(this.M11, endian);
            output.WriteValueF32(this.M12, endian);
            output.WriteValueF32(this.M13, endian);
            output.WriteValueF32(this.M21, endian);
            output.WriteValueF32(this.M22, endian);
            output.WriteValueF32(this.M23, endian);
            output.WriteValueF32(this.M31, endian);
            output.WriteValueF32(this.M32, endian);
            output.WriteValueF32(this.M33, endian);
            output.WriteValueF32(this.M41, endian);
            output.WriteValueF32(this.M42, endian);
            output.WriteValueF32(this.M43, endian);
        }


        void IRawVariant.Deserialize(Stream input, Endian endian)
        {
            this.M11 = input.ReadValueF32(endian);
            this.M12 = input.ReadValueF32(endian);
            this.M13 = input.ReadValueF32(endian);
            this.M21 = input.ReadValueF32(endian);
            this.M22 = input.ReadValueF32(endian);
            this.M23 = input.ReadValueF32(endian);
            this.M31 = input.ReadValueF32(endian);
            this.M32 = input.ReadValueF32(endian);
            this.M33 = input.ReadValueF32(endian);
            this.M41 = input.ReadValueF32(endian);
            this.M42 = input.ReadValueF32(endian);
            this.M43 = input.ReadValueF32(endian);
        }
    }
}
