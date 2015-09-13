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

namespace Gibbed.MadMax.PropertyFormats
{
    public static class VariantFactory
    {
        public static IVariant GetVariant(string type)
        {
            switch (type)
            {
                case "int":
                {
                    return new Variants.IntegerVariant();
                }

                case "float":
                {
                    return new Variants.FloatVariant();
                }

                case "string":
                {
                    return new Variants.StringVariant();
                }

                case "vec2":
                {
                    return new Variants.Vector2Variant();
                }

                case "vec":
                {
                    return new Variants.Vector3Variant();
                }

                case "vec4":
                {
                    return new Variants.Vector4Variant();
                }

                case "mat":
                {
                    return new Variants.Matrix4x3Variant();
                }

                case "vec_int":
                {
                    return new Variants.IntegersVariant();
                }

                case "vec_float":
                {
                    return new Variants.FloatsVariant();
                }

                case "vec_events":
                {
                    return new Variants.EventsVariant();
                }
            }

            throw new ArgumentException("unknown variant type", "type");
        }

        internal static IRawVariant GetVariant(RawVariantType type)
        {
            switch (type)
            {
                case RawVariantType.Integer:
                {
                    return new Variants.IntegerVariant();
                }

                case RawVariantType.Float:
                {
                    return new Variants.FloatVariant();
                }

                case RawVariantType.String:
                {
                    return new Variants.StringVariant();
                }

                case RawVariantType.Vector2:
                {
                    return new Variants.Vector2Variant();
                }

                case RawVariantType.Vector3:
                {
                    return new Variants.Vector3Variant();
                }

                case RawVariantType.Vector4:
                {
                    return new Variants.Vector4Variant();
                }

                case RawVariantType.Matrix4x3:
                {
                    return new Variants.Matrix4x3Variant();
                }

                case RawVariantType.Integers:
                {
                    return new Variants.IntegersVariant();
                }

                case RawVariantType.Floats:
                {
                    return new Variants.FloatsVariant();
                }

                case RawVariantType.Events:
                {
                    return new Variants.EventsVariant();
                }
            }

            throw new ArgumentException("unknown variant type", "type");
        }
    }
}
