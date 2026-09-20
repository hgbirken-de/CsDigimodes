using DigitalVoice.Common;

namespace DigitalVoice.Dmr;

/// <summary>
/// Accumulates the 128-bit embedded LC codeword from voice frames B-E
/// (seqNo 1-4) of one superframe, then decodes it via
/// <see cref="DmrCodec.DecodeEmbeddedData"/>. Exact inverse of
/// <see cref="DmrCodec.WriteEmbeddedData"/>.
/// </summary>
public class EmbeddedLcAccumulator
{
    bool[] _bits = new bool[128];
    readonly HashSet<int> _seen = [];

    /// <summary>
    /// Resets any partially-accumulated fragments, discarding them.
    /// </summary>
    public void Reset()
    {
        _bits = new bool[128];
        _seen.Clear();
    }

    /// <summary>
    /// Feed one received voice frame (B-E, seqNo 1-4). Once all 4 fragments
    /// of the current superframe have arrived, decodes and returns the 9-byte
    /// LC data. Returns <c>null</c> while fragments are still missing.
    /// </summary>
    /// <param name="dmrPkt">The full DMR frame payload.</param>
    /// <param name="seqNo">The frame's position within the superframe (0=A, 1-4=B..E, 5=F).</param>
    public byte[]? AddFrame(byte[] dmrPkt, int seqNo)
    {
        if (seqNo == 0)
        {
            Reset(); // new superframe (voice sync) - discard stale partial data
            return null;
        }
        if (seqNo < 1 || seqNo > 4)
        {
            return null; // frame F carries no embedded LC data
        }

        byte[] ei = new byte[6];
        DmrCodec.ExtractEiFromFrame(dmrPkt, 20, ei);

        int n = seqNo - 1;
        bool[] byteBits = new bool[8];
        for (int i = 1; i <= 4; i++) // fragment = ei[1..4]
        {
            DmrCodec.ByteToBitsBE(ei[i], byteBits);
            int bitOffset = n * 32 + (i - 1) * 8;
            Array.Copy(byteBits, 0, _bits, bitOffset, 8);
        }

        _seen.Add(seqNo);

        if (_seen.IsSupersetOf(new[] { 1, 2, 3, 4 }))
        {
            byte[]? lcData = DmrCodec.DecodeEmbeddedData(_bits);
            Reset();
            return lcData;
        }

        return null;
    }
}