namespace DigitalVoice.Nxdn;

internal static class BitOperations
{
    static readonly byte[] BIT_MASK = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    /// <summary>
    /// Checkss a single bit in a packed bitstream is set.
    /// </summary>
    /// <param name="ba">The byte array containing the packed bitstream (MSB-first in each byte).</param>
    /// <param name="bitIdx">The zero-based bit index to read (0 = first bit of <paramref name="ba"/>[0]).</param>
    /// <returns>true if the bit at position <paramref name="bitIdx"/> is set, otherwise false.</returns>
    internal static bool ReadBit(byte[] ba, int bitIdx)
    {
        return (ba[bitIdx >> 3] & BIT_MASK[bitIdx & 7]) != 0;
    }

    /// <summary>
    /// Reads a single bit from a packed bitstream.
    /// </summary>
    /// <param name="ba">The byte array containing the packed bitstream (MSB-first in each byte).</param>
    /// <param name="bitIdx">The zero-based bit index to read (0 = first bit of <paramref name="ba"/>[0]).</param>
    /// <returns>1 if the bit at position <paramref name="bitIdx"/> is set, otherwise 0.</returns>
    internal static int ReadBit1(byte[] ba, int bitIdx)
    {
        return (ba[bitIdx >> 3] & BIT_MASK[bitIdx & 7]) != 0 ? 1 : 0;
    }

    /// <summary>
    /// Writes a single bit into a packed bitstream.
    /// </summary>
    /// <param name="ba">The byte array containing the packed bitstream (MSB-first in each byte).</param>
    /// <param name="bitIdx">The zero-based bit index to write (0 = first bit of <paramref name="ba"/>[0]).</param>
    /// <param name="value">The bit value to write: <c>true</c> to set the bit to 1, <c>false</c> to clear it to 0.</param>
    internal static void WriteBit1(byte[] ba, int bitIdx, bool value)
    {
        ba[bitIdx >> 3] = value ? (byte)(ba[bitIdx >> 3] | BIT_MASK[bitIdx & 7]) : (byte)(ba[bitIdx >> 3] & ~BIT_MASK[bitIdx & 7]);
    }
}
