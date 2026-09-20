using NLog;

namespace DigitalVoice.Dmr;

/// <summary>
/// Collect Embedded Information (EI) bytes from DMR voice frames (B..F),
/// assemble 9-byte LC PDUs, de-whiten them and parse FLCO / Talker Alias header/block.
/// Intended as a diagnostic helper: it logs PDUs, TA headers and TA blocks it finds.
/// </summary>
public class EiPduCollector
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // per-srcId buffer of EI bytes (append-only, trimmed)
    // keying by srcId is a minimum; you may want srcId+slot if you have multi-slot usage
    readonly Dictionary<int, List<byte>> _eiBuffers = new();

    // keep a bounded buffer per source to avoid runaway memory use
    const int MaxBufferBytesPerSource = 256;

    /// <summary>
    /// Process a received DMR packet (full 55-byte packet as you already have it).
    /// Call this from your packet handling path.
    /// </summary>
    /// <param name="srcId">source DMR ID (from header)</param>
    /// <param name="dmrPkt">full packet (55 bytes)</param>
    /// <param name="payloadOffset">offset to the 33-byte DMR frame (default 20)</param>
    public void ProcessPacket(int srcId, byte[] dmrPkt, int payloadOffset = 20)
    {
        if (dmrPkt == null) return;
        if (dmrPkt.Length < payloadOffset + 33)
        {
            logger.Error($"DMR packet too short ({dmrPkt.Length})");
            return;
        }

        // flag bits live inside the 20-byte header (you said dmrPkt[15])
        byte flagByte = dmrPkt[15];
        int burstIndex = flagByte & 0x0F; // 0=A,1=B,2=C,3=D,4=E,5=F
        bool isVoiceSync = (burstIndex == 0); // A (VoiceSync) -> superframe start

        if (isVoiceSync) return; // NO TA data here

        // Extract the 33-byte DMR frame
        Span<byte> frame = dmrPkt.AsSpan(payloadOffset, 33);

        // Extract the 6 EI bytes (48 bits) from the 33-byte frame
        // EI bits are located across low nibble of frame[13], full frame[14..18], high nibble of frame[19]
        byte[] ei = new byte[6];
        ExtractEiFromFrame(frame, ei);

        logger.Debug($"[EI] srcId={srcId} burst={burstIndex} ei={Convert.ToHexString(ei)}");

        // Append EI bytes to buffer for this srcId
        if (!_eiBuffers.TryGetValue(srcId, out var buf))
        {
            buf = [];
            _eiBuffers[srcId] = buf;
        }

        buf.AddRange(ei);

        // Trim buffer if it grows too large
        if (buf.Count > MaxBufferBytesPerSource)
            buf.RemoveRange(0, buf.Count - MaxBufferBytesPerSource);

        // Try to parse as many 9-byte LC PDUs as possible (they are byte-aligned)
        // PDUs in EI are whitened: we will de-whiten (XOR 0x55) before parsing.
        while (buf.Count >= 9)
        {
            byte[] candidate = buf.Take(9).ToArray();

            // De-whiten the 9 bytes (per ETSI TS 102 361-2)
            //for (int i = 0; i < candidate.Length; i++)
            //    candidate[i] ^= 0x55;

            // FLCO is stored in the low 6 bits of first byte (spec: 6-bit opcode)
            int flco = candidate[0] & 0x3F;

            // Log raw PDU (post-dewhiten)
            logger.Debug($"[PDU] srcId={srcId} pdu={Convert.ToHexString(candidate)} flco={flco}");

            // check for TA FLCOs (header=4, blocks=5/6/7)
            if (flco == 4)
            {
                ParseTaHeaderPdu(srcId, candidate);
            }
            else if (flco >= 5 && flco <= 7)
            {
                ParseTaBlockPdu(srcId, candidate, flco - 4); // blockIndex 1..3
            }
            else
            {
                // Not a TA PDU — you can extend here to parse other LC PDUs (CSBK, etc.)
                logger.Debug($"[PDU] srcId={srcId} non-TA FLCO={flco}");
            }

            // consume the 9 bytes from buffer
            buf.RemoveRange(0, 9);
        }
    }

    /// <summary>
    /// Extract the 48 EI bits (6 bytes) from the 33-byte DMR frame.
    /// The frame is MSB-first in bytes. This packs the EI bytes MSB-first.
    /// </summary>
    static void ExtractEiFromFrame(ReadOnlySpan<byte> f, Span<byte> eiOut)
    {
        if (f.Length < 33) throw new ArgumentException("frame must be 33 bytes");
        if (eiOut.Length < 6) throw new ArgumentException("eiOut must be 6 bytes");

        // See explanation earlier: low nibble of f[13], then f[14]..f[18], then high nibble of f[19].
        eiOut[0] = (byte)(((f[13] & 0x0F) << 4) | (f[14] >> 4));
        eiOut[1] = (byte)(((f[14] & 0x0F) << 4) | (f[15] >> 4));
        eiOut[2] = (byte)(((f[15] & 0x0F) << 4) | (f[16] >> 4));
        eiOut[3] = (byte)(((f[16] & 0x0F) << 4) | (f[17] >> 4));
        eiOut[4] = (byte)(((f[17] & 0x0F) << 4) | (f[18] >> 4));
        eiOut[5] = (byte)(((f[18] & 0x0F) << 4) | (f[19] >> 4));
    }

    static void ParseTaHeaderPdu(int srcId, byte[] pdu)
    {
        // pdu length must be 9 (already enforced by caller)
        // Byte layout:
        // pdu[0] : FLCO + other bits (already used)
        // pdu[1] : PF | (format:2 bits) | (size:5 bits)
        // pdu[2..8] : 7 bytes of TA data (49 bits, but top bit reserved for some formats)
        int taFormat = (pdu[1] >> 6) & 0x03;
        int taSize = (pdu[1] >> 1) & 0x1F;

        // copy the 7 bytes of alias data
        byte[] taHeaderData = new byte[7];
        Array.Copy(pdu, 2, taHeaderData, 0, 7);

        // For formats != 0 (7-bit), the MSB of the 49-bit field is reserved.
        if (taFormat != 0)
            taHeaderData[0] &= 0x7F;

        logger.Debug($"[TA_HEADER] srcId={srcId}, format={taFormat}, size={taSize}, headerData={Convert.ToHexString(taHeaderData)}");
    }

    static void ParseTaBlockPdu(int srcId, byte[] pdu, int blockIndex)
    {
        // pdu[2..8] are block data (7 bytes = 56 bits)
        byte[] blockData = new byte[7];
        Array.Copy(pdu, 2, blockData, 0, 7);
        logger.Debug($"[TA_BLOCK{blockIndex}] srcId={srcId}, data={Convert.ToHexString(blockData)}");
    }
}
