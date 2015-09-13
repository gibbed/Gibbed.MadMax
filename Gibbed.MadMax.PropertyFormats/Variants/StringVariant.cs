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
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.MadMax.PropertyFormats.Variants
{
    public class StringVariant : IVariant, IRawVariant
    {
        public string Value = "";

        public string Tag
        {
            get { return "string"; }
        }

        public void Parse(string text)
        {
            this.Value = text;
        }

        public string Compose()
        {
            return this.Value;
        }

        RawVariantType IRawVariant.Type
        {
            get { return RawVariantType.String; }
        }

        void IRawVariant.Serialize(Stream output, Endian endian)
        {
            string s = this.Value;
            if (s.Length > 0xFFFF)
            {
                throw new InvalidOperationException();
            }

            output.WriteValueU16((ushort)s.Length, endian);
            output.WriteString(s, Encoding.ASCII);
        }

        void IRawVariant.Deserialize(Stream input, Endian endian)
        {
            ushort length = input.ReadValueU16(endian);
            this.Value = input.ReadString(length, true, Encoding.ASCII);
        }
    }
}
