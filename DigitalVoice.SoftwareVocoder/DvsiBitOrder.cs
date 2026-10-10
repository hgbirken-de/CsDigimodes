namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Bitreihenfolge der 49-Bit-Frames (YSF, FCS, NXDN). Der DVSI-Chip erwartet die 49 AMBE-Bits im Channel-Paket in einer
/// anderen Reihenfolge, als sie in der Luft übertragen werden und als die mbelib sie braucht ("Luft-Reihenfolge").
/// Die Clients ordnen die Bits für den Chip um (<c>NxdnCodec.Interleave</c>, <c>AmbeExtractor</c> im Fusion-Codec). Ein
/// Software-Decoder, der dieselben Pakete bekommt, muss die Umordnung rückgängig machen.
/// <para>
/// Die Tabelle ist <c>dvsi_interleave</c> aus DroidStar (<c>nxdn.cpp</c>, <c>ysf.cpp</c>): Das Luftbit <c>i</c> liegt im
/// Chip-Paket an der Bitposition <c>Table[i]</c>. Die Bits sind MSB-first in 7 Bytes abgelegt, Bit 48 im höchsten Bit von
/// Byte 6.
/// </para>
/// </summary>
internal static class DvsiBitOrder
{
    private static readonly byte[] Table =
    [
        0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 41, 43, 45, 47,
        1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34, 37, 40, 42, 44, 46, 48,
        2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35, 38,
    ];

    /// <summary>Chip-Reihenfolge (wie im DV3000-Channel-Paket) nach Luft-Reihenfolge (wie sie der Decoder braucht).</summary>
    public static void ToAirOrder(ReadOnlySpan<byte> dvsi, Span<byte> air)
    {
        for (int i = 0; i < 7; i++)
            air[i] = 0;

        for (int i = 0; i < 49; i++)
        {
            int position = Table[i];
            int bit = (dvsi[position >> 3] >> (7 - (position & 7))) & 1;
            air[i >> 3] |= (byte)(bit << (7 - (i & 7)));
        }
    }

    /// <summary>Luft-Reihenfolge nach Chip-Reihenfolge (die Richtung, in der die Clients umordnen).</summary>
    public static void ToDvsiOrder(ReadOnlySpan<byte> air, Span<byte> dvsi)
    {
        for (int i = 0; i < 7; i++)
            dvsi[i] = 0;

        for (int i = 0; i < 49; i++)
        {
            int position = Table[i];
            int bit = (air[i >> 3] >> (7 - (i & 7))) & 1;
            dvsi[position >> 3] |= (byte)(bit << (7 - (position & 7)));
        }
    }
}
