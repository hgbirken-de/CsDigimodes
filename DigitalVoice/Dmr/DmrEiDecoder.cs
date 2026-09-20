namespace DigitalVoice.Dmr;

public static class DmrEiDecoder
{
    /// <summary>
    /// Generate PRBS (x^9 + x^5 + 1) sequence.
    /// </summary>
    private static IEnumerable<int> GeneratePrbs(int count, ushort seed = 0x1FF)
    {
        ushort reg = seed; // 9-bit LFSR, init non-zero (0x1FF is common)
        for (int i = 0; i < count; i++)
        {
            int newBit = ((reg >> 4) ^ (reg >> 8)) & 1; // taps at positions 5 and 9
            reg = (ushort)(((reg << 1) | newBit) & 0x1FF);
            yield return (reg >> 8) & 1; // output MSB
        }
    }

    /// <summary>
    /// Dewhiten 6 EI bytes (48 bits).
    /// Returns a 6-byte array containing dewhitened bits.
    /// </summary>
    public static byte[] DewhitenEi(byte[] ei)
    {
        if (ei == null || ei.Length != 6)
            throw new ArgumentException("ei must be 6 bytes");

        var output = new byte[6];
        var prbs = GeneratePrbs(48).ToArray();

        for (int i = 0; i < 48; i++)
        {
            int byteIndex = i / 8;
            int bitIndex = 7 - (i % 8); // big-endian bit order

            // Extract input bit
            int bit = (ei[byteIndex] >> bitIndex) & 1;

            // XOR with PRBS bit
            bit ^= prbs[i];

            // Store in output
            output[byteIndex] |= (byte)(bit << bitIndex);
        }

        return output;
    }
}

