namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Fehlerkorrektur (Golay 23/12): Port von <c>ecc.c</c> der mbelib. Bits werden als <see cref="int"/>-Werte 0/1 in Spans
/// übergeben (im C-Original <c>char</c>-Felder).
/// </summary>
internal static class Ecc
{
    /// <summary>Korrigiert einen Golay-Block und liefert die 12 Datenbits (<c>mbe_checkGolayBlock</c>).</summary>
    private static int CheckGolayBlock(long block)
    {
        long mask = 0x400000L;
        int eccexpected = 0;
        for (int i = 0; i < 12; i++)
        {
            if ((block & mask) != 0L)
                eccexpected ^= AmbeTables.golayGenerator[i];
            mask >>= 1;
        }

        int eccbits = (int)(block & 0x7ffL);
        int syndrome = eccexpected ^ eccbits;

        int databits = (int)(block >> 11);
        databits ^= AmbeTables.golayMatrix[syndrome];
        return databits;
    }

    /// <summary>
    /// Golay-Dekodierung eines 23-Bit-Blocks (<c>mbe_golay2312</c>). <paramref name="input"/> und <paramref name="output"/>
    /// haben mindestens 23 Elemente.
    /// </summary>
    /// <returns>Anzahl der korrigierten Bits in den Datenbits.</returns>
    public static int Golay2312(ReadOnlySpan<int> input, Span<int> output)
    {
        long block = 0;
        for (int i = 22; i >= 0; i--)
        {
            block <<= 1;
            block += input[i];
        }

        block = CheckGolayBlock(block);

        for (int i = 22; i >= 11; i--)
        {
            output[i] = (int)((block & 2048) >> 11);
            block <<= 1;
        }
        for (int i = 10; i >= 0; i--)
            output[i] = input[i];

        int errs = 0;
        for (int i = 22; i >= 11; i--)
        {
            if (output[i] != input[i])
                errs++;
        }
        return errs;
    }
}
