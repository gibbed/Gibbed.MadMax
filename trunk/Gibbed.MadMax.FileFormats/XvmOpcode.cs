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

namespace Gibbed.MadMax.FileFormats
{
    public enum XvmOpcode : ushort
    {
        Assert = 0,
        And = 1,
        Or = 2,
        Add = 3,
        Div = 4,
        Mod = 5,
        Mul = 6,
        Sub = 7,
        MakeList = 8,
        Call = 9,
        CmpEq = 10,
        CmpGe = 11,
        CmpG = 12,
        CmpNe = 13,
        Jmp = 14,
        Jz = 15,
        LoadAttr = 18,
        LoadConst = 19,
        LoadBool = 20,
        LoadGlobal = 21,
        LoadLocal = 22,
        LoadItem = 23,
        Pop = 24,
        DebugOut = 25,
        Ret = 26,
        StoreAttr = 27,
        StoreLocal = 28,
        StoreItem = 29,
        IsZero = 30,
        Neg = 31,
    }
}
