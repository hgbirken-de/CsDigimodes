
using static FusionCodec.BitOperations;

namespace FusionCodec;

/// <summary>
/// Provides an alternative implementation for extracting a 7-byte AMBE block
/// from a 13-byte YSF voice data frame using byte-level bit manipulation.
/// </summary>
public static class AmbeExtractor2
{
    /// <summary>
    /// Extracts and reconstructs a 7-byte AMBE block from a 13-byte YSF voice data segment.
    ///
    /// The extraction process performs the following steps:
    /// <list type="number">
    ///   <item>Deinterleaves the 104-bit encoded data using the 26×4 interleave table.</item>
    ///   <item>Applies a whitening mask via XOR using a predefined whitening sequence.</item>
    ///   <item>Performs error correction using majority voting over 27 triplets (first 81 bits).</item>
    ///   <item>Copies the remaining 22 bits directly from the deinterleaved buffer.</item>
    ///   <item>Reinterleaves the 49 corrected bits using a final AMBE output interleave table.</item>
    /// </list>
    /// </summary>
    /// <param name="ysfVoiceData">A 13-byte YSF voice data payload from a YSF data frame.</param>
    /// <returns>A 7-byte AMBE-encoded block, suitable for decoding by an AMBE vocoder.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="ysfVoiceData"/> is not exactly 13 bytes.</exception>
    public static byte[] ExtractAmbe(byte[] ysfVoiceData)
    {
        if (ysfVoiceData.Length != 13)
            throw new ArgumentException("YSF voice data block must be exactly 13 bytes.");

        // Step 1 & 2: Deinterleave 104 bits into separate buffer
        byte[] deinterleaved = new byte[13];
        for (int i = 0; i < 104; i++)
        {
            bool bit = ReadBit1(ysfVoiceData, Decoder.INTERLEAVE_TABLE_26_4[i]) != 0;
            WriteBit1(deinterleaved, i, bit);
        }

        // Step 3: Whitening by XOR
        for (int i = 0; i < 13; i++)
        {
            deinterleaved[i] ^= Decoder.WHITENING_DATA[i];
        }

        // Step 4: Error correction (majority vote)
        byte[] correctedBits = new byte[7]; // 49 bits
        int bitIndex = 0;
        for (int i = 0; i < 27; i++)
        {
            int count = 0;
            for (int j = 0; j < 3; j++)
            {
                if (ReadBit1(deinterleaved, bitIndex + j) != 0)
                    count++;
            }
            WriteBit1(correctedBits, i, count >= 2);
            bitIndex += 3;
        }

        // Step 5: Copy remaining 22 bits from deinterleaved
        for (int i = 0; i < 22; i++)
        {
            bool bit = ReadBit1(deinterleaved, 81 + i) != 0;
            WriteBit1(correctedBits, 27 + i, bit);
        }

        // Step 6: Final interleave
        byte[] finalBits = new byte[7]; // 49 bits
        for (int i = 0; i < 49; i++)
        {
            bool bit = ReadBit1(correctedBits, Decoder.AMBE_OUTPUT_BIT_INTERLEAVE_TABLE[i]) != 0;
            WriteBit1(finalBits, i, bit);
        }

        return finalBits;
    }
}
