namespace FusionCodec;

/// <summary>
/// Provides functionality to extract and decode AMBE voice data blocks from Yaesu System Fusion (YSF) voice packets.
/// </summary>
public static class AmbeExtractor1
{
    /// <summary>
    /// Extracts a 7-byte AMBE block from a 13-byte YSF voice data segment.
    ///
    /// The process includes:
    /// <list type="number">
    ///   <item>Converting input bytes to a bit array (MSB first).</item>
    ///   <item>Deinterleaving the first 104 bits using a 26×4 interleave table.</item>
    ///   <item>Removing the whitening mask using XOR.</item>
    ///   <item>Applying majority-vote error correction across 27 triplets (81 bits).</item>
    ///   <item>Copying the remaining 22 bits unchanged.</item>
    ///   <item>Reinterleaving the resulting 49 bits using a custom AMBE interleave table.</item>
    ///   <item>Packing the final 49 bits into a 7-byte AMBE block.</item>
    /// </list>
    /// </summary>
    /// <param name="ysfVoiceData">A 13-byte voice payload extracted from a YSF voice frame (excluding header bytes).</param>
    /// <returns>The decoded 7-byte AMBE block ready for decoding by an AMBE vocoder.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="ysfVoiceData"/> is not exactly 13 bytes.</exception>
    public static byte[] ExtractAmbe(byte[] ysfVoiceData)
    {
        if (ysfVoiceData.Length != 13)
            throw new ArgumentException("YSF voice data block must be exactly 13 bytes.");

        // 1) Convert to bits (MSB first)
        bool[] bits = BytesToBits(ysfVoiceData);

        // 2) Deinterleave using INTERLEAVE_TABLE_26_4 (104 bits)
        bool[] deinterleaved = new bool[104];
        for (int i = 0; i < 104; i++)
        {
            deinterleaved[i] = bits[Decoder.INTERLEAVE_TABLE_26_4[i]];
        }

        // 3) Convert bits back to bytes for whitening
        byte[] whitenedBytes = ToByteArray(deinterleaved, 13);

        // 4) Remove whitening by XOR
        for (int i = 0; i < 13; i++)
        {
            whitenedBytes[i] ^= Decoder.WHITENING_DATA[i];
        }

        // 5) Convert whitened bytes back to bits for error correction
        bool[] whitenedBits = BytesToBits(whitenedBytes);

        // 6) Error correction with majority vote on every 3 bits for first 81 bits (27 groups)
        bool[] correctedBits = new bool[49];
        int bitIndex = 0;
        for (int i = 0; i < 27; i++)
        {
            int onesCount = 0;
            for (int j = 0; j < 3; j++)
            {
                if (whitenedBits[bitIndex + j]) onesCount++;
            }

            // Majority vote: if ones >= 2, bit = true; else false
            bool val = onesCount >= 2;
            correctedBits[i] = val;

            bitIndex += 3;
        }

        // 7) Copy last 22 bits (from bit 81 to 102) unchanged
        Array.Copy(whitenedBits, 81, correctedBits, 27, 22);

        // 8) Final interleave using INTER_TABBIE_VCH49 (49 bits)
        bool[] finalBits = new bool[49];
        for (int i = 0; i < 49; i++)
        {
            finalBits[i] = correctedBits[Decoder.AMBE_OUTPUT_BIT_INTERLEAVE_TABLE[i]];
        }

        // 9) Convert final bits to bytes (7 bytes)
        return ToByteArray(finalBits, 7);
    }

    /// <summary>
    /// Convert bytes to bits (MSB first)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static bool[] BytesToBits(byte[] data)
    {
        bool[] bits = new bool[data.Length * 8];
        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];
            for (int j = 0; j < 8; j++)
            {
                bits[i * 8 + j] = (b & 1 << 7 - j) != 0;
            }
        }
        return bits;
    }

    /// <summary>
    /// Convert bits to bytes (MSB first), byteCount = how many bytes in output
    /// </summary>
    /// <param name="bits"></param>
    /// <param name="byteCount"></param>
    /// <returns></returns>
    private static byte[] ToByteArray(bool[] bits, int byteCount)
    {
        byte[] bytes = new byte[byteCount];
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i])
            {
                bytes[i / 8] |= (byte)(1 << 7 - i % 8);
            }
        }
        return bytes;
    }
}