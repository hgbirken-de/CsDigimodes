// Port of the fixed-point basic operators and math helpers of the "Project 25 IMBE Encoder/Decoder Fixed-Point
// implementation", Developed by Pavel Yazev <pyazev@gmail.com>, Version 1.0 (c) Copyright 2009.
// (basicop2.cc, math_sub.cc in the version contained in DroidStar.)
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later
// version. It is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program (LICENSE-GPL.txt).

using System.Runtime.CompilerServices;

namespace DigitalVoice.SoftwareVocoder.Imbe;

/// <summary>
/// Festkomma-Grundoperationen (ITU-T/ETSI-Basisoperatoren) mit Sättigung. Die Namen entsprechen dem C-Original
/// (<c>add</c> = <see cref="Add"/>, <c>L_mult</c> = <see cref="LMult"/> und so weiter). <c>Word16</c> ist <see cref="short"/>,
/// <c>Word32</c> ist <see cref="int"/>. Das Überlauf-Flag des Originals wird nirgends gelesen und entfällt.
/// </summary>
internal static class Fx
{
    public const short Max16 = 0x7fff;
    public const short Min16 = -0x8000;
    public const int Max32 = int.MaxValue;
    public const int Min32 = int.MinValue;

    private const int MaskHigh17 = unchecked((int)0xffff8000);   // (Word32)0xffff8000L

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Saturate(int value)
    {
        if (value > 0x7fff) return Max16;
        if (value < -0x8000) return Min16;
        return (short)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Add(short a, short b) => Saturate(a + b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Sub(short a, short b) => Saturate(a - b);

    public static short Shl(short var1, int shift)
    {
        short var2 = (short)shift;
        if (var2 < 0)
        {
            if (var2 < -16) var2 = -16;
            return Shr(var1, -var2);
        }

        int result = var1 * (1 << var2);
        if ((var2 > 15 && var1 != 0) || result != (short)result)
            return var1 > 0 ? Max16 : Min16;

        return (short)result;
    }

    public static short Shr(short var1, int shift)
    {
        short var2 = (short)shift;
        if (var2 < 0)
        {
            if (var2 < -16) var2 = -16;
            return Shl(var1, -var2);
        }

        if (var2 >= 15)
            return (short)(var1 < 0 ? -1 : 0);

        return var1 < 0 ? (short)~((~var1) >> var2) : (short)(var1 >> var2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Mult(short var1, short var2)
    {
        int product = var1 * var2;
        product = (product & MaskHigh17) >> 15;
        if ((product & 0x00010000) != 0)
            product |= unchecked((int)0xffff0000);
        return Saturate(product);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short MultR(short var1, short var2)
    {
        int product = var1 * var2;
        product += 0x00004000;
        product &= MaskHigh17;
        product >>= 15;
        if ((product & 0x00010000) != 0)
            product |= unchecked((int)0xffff0000);
        return Saturate(product);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Negate(short var1) => var1 == Min16 ? Max16 : (short)-var1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LAdd(int a, int b)
    {
        int result = unchecked(a + b);
        if (((a ^ b) & Min32) == 0 && ((result ^ a) & Min32) != 0)
            result = a < 0 ? Min32 : Max32;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LSub(int a, int b)
    {
        int result = unchecked(a - b);
        if (((a ^ b) & Min32) != 0 && ((result ^ a) & Min32) != 0)
            result = a < 0 ? Min32 : Max32;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LMult(short var1, short var2)
    {
        int result = var1 * var2;
        return result != 0x40000000 ? result * 2 : Max32;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LMac(int accumulator, short var1, short var2) => LAdd(accumulator, LMult(var1, var2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LMsu(int accumulator, short var1, short var2) => LSub(accumulator, LMult(var1, var2));

    public static int LShl(int value, int shift)
    {
        short var2 = (short)shift;
        if (var2 <= 0)
        {
            if (var2 < -32) var2 = -32;
            return LShr(value, -var2);
        }

        int result = 0;
        for (; var2 > 0; var2--)
        {
            if (value > 0x3fffffff)
            {
                result = Max32;
                break;
            }

            if (value < unchecked((int)0xc0000000))
            {
                result = Min32;
                break;
            }

            value *= 2;
            result = value;
        }

        return result;
    }

    public static int LShr(int value, int shift)
    {
        short var2 = (short)shift;
        if (var2 < 0)
        {
            if (var2 < -32) var2 = -32;
            return LShl(value, -var2);
        }

        if (var2 >= 31)
            return value < 0 ? -1 : 0;

        return value < 0 ? ~((~value) >> var2) : value >> var2;
    }

    public static int LShrR(int value, int shift)
    {
        short var2 = (short)shift;
        if (var2 > 31)
            return 0;

        int result = LShr(value, var2);
        if (var2 > 0 && (value & (1 << (var2 - 1))) != 0)
            result++;
        return result;
    }

    public static short ShrR(short var1, int shift)
    {
        short var2 = (short)shift;
        if (var2 > 15)
            return 0;

        short result = Shr(var1, var2);
        if (var2 > 0 && (var1 & (1 << (var2 - 1))) != 0)
            result++;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Round(int value) => ExtractH(LAdd(value, 0x00008000));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ExtractH(int value) => (short)(value >> 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ExtractL(int value) => (short)value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LDepositH(short var1) => var1 << 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LDepositL(short var1) => var1;

    public static short NormS(short var1)
    {
        if (var1 == 0) return 0;
        if (var1 == -1) return 15;
        if (var1 < 0) var1 = (short)~var1;

        short count = 0;
        for (; var1 < 0x4000; count++)
            var1 = (short)(var1 << 1);
        return count;
    }

    public static short NormL(int value)
    {
        if (value == 0) return 0;
        if (value == -1) return 31;
        if (value < 0) value = ~value;

        short count = 0;
        for (; value < 0x40000000; count++)
            value <<= 1;
        return count;
    }

    /// <summary>
    /// Division <c>var1 / var2</c> im Q15-Format. Das C-Original bricht bei ungültigen Werten (<c>var1 &gt; var2</c>, negative
    /// Werte, Division durch 0) das Programm ab. Hier liefert die Funktion in diesen Fällen den größten Wert.
    /// </summary>
    public static short DivS(short var1, short var2)
    {
        if (var1 > var2 || var1 < 0 || var2 < 0 || var2 == 0)
            return Max16;

        if (var1 == 0) return 0;
        if (var1 == var2) return Max16;

        short result = 0;
        int numerator = var1;
        int denominator = var2;
        for (short iteration = 0; iteration < 15; iteration++)
        {
            result = (short)(result << 1);
            numerator <<= 1;
            if (numerator >= denominator)
            {
                numerator = LSub(numerator, denominator);
                result = Add(result, 1);
            }
        }

        return result;
    }

    public static int LMpyLs(int var2, short var1)
    {
        short swtemp = Shr(ExtractL(var2), 1);
        swtemp = (short)(32767 & swtemp);
        int result = LMult(var1, swtemp);
        result = LShr(result, 15);
        result = LMac(result, var1, ExtractH(var2));
        return result;
    }

    // ------------------------------------------------------------------
    // math_sub.cc
    // ------------------------------------------------------------------

    private const short X05Q15 = 16384;
    public const short OneQ15 = 32767;

    public static short CosFxp(short x)
    {
        short sign = 0;
        short tx = x < 0 ? Negate(x) : x;

        if (tx > X05Q15)
        {
            tx = Sub(OneQ15, tx);
            sign = -1;
        }

        short index1 = Shr(tx, 7);
        short index2 = Add(index1, 1);
        if (index1 == 128)
            return 0;

        short m = Sub(tx, Shl(index1, 7));
        m = Shl(m, 8);
        short temp = Sub(ImbeTables.CosTable[index2], ImbeTables.CosTable[index1]);
        temp = Mult(m, temp);
        short ty = Add(ImbeTables.CosTable[index1], temp);

        return sign != 0 ? Negate(ty) : ty;
    }

    public static short SinFxp(short x)
    {
        short sign = 0;
        short tx;
        if (x < 0)
        {
            tx = Negate(x);
            sign = 1;
        }
        else
        {
            tx = x;
        }

        tx = tx > X05Q15 ? Sub(tx, X05Q15) : Sub(X05Q15, tx);

        short ty = CosFxp(tx);
        return sign != 0 ? Negate(ty) : ty;
    }

    /// <summary>Wurzel mit Exponent (<c>sqrt_l_exp</c>).</summary>
    public static int SqrtLExp(int x, out short exp)
    {
        if (x <= 0)
        {
            exp = 0;
            return 0;
        }

        short e = (short)(NormL(x) & 0xFFFE);
        x = LShl(x, e);
        exp = (short)(e >> 1);
        x = LShr(x, 9);
        short i = ExtractH(x);
        x = LShr(x, 1);
        short a = ExtractL(x);
        a = (short)(a & 0x7fff);
        i = Sub(i, 16);

        int y = LDepositH(ImbeTables.SqrtTable[i]);
        short tmp = Sub(ImbeTables.SqrtTable[i], ImbeTables.SqrtTable[i + 1]);
        y = LMsu(y, tmp, a);
        return y;
    }
}
