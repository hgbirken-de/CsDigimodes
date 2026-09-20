using DigitalVoice.Common;
using NLog;
using System;
using System.Collections;
using System.Text;

namespace DigitalVoice.Dmr;

public class DmrCodec
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Precomputed lookup table (128 entries) for the QR(16,7,6) block code.
    /// Each entry maps 7 information bits (0..127) to a 16-bit encoded codeword.
    /// Source: ETSI TS 102 361-1 V2.6.1 (2023-05), B.3.2 Quadratic residue (16,7,6).
    /// </summary>
    readonly static int[] EncodingTable1676 =
        [0x0000, 0x0273, 0x04E5, 0x0696, 0x09C9, 0x0BBA, 0x0D2C, 0x0F5F, 0x11E2, 0x1391, 0x1507, 0x1774,
         0x182B, 0x1A58, 0x1CCE, 0x1EBD, 0x21B7, 0x23C4, 0x2552, 0x2721, 0x287E, 0x2A0D, 0x2C9B, 0x2EE8,
         0x3055, 0x3226, 0x34B0, 0x36C3, 0x399C, 0x3BEF, 0x3D79, 0x3F0A, 0x411E, 0x436D, 0x45FB, 0x4788,
         0x48D7, 0x4AA4, 0x4C32, 0x4E41, 0x50FC, 0x528F, 0x5419, 0x566A, 0x5935, 0x5B46, 0x5DD0, 0x5FA3,
         0x60A9, 0x62DA, 0x644C, 0x663F, 0x6960, 0x6B13, 0x6D85, 0x6FF6, 0x714B, 0x7338, 0x75AE, 0x77DD,
         0x7882, 0x7AF1, 0x7C67, 0x7E14, 0x804F, 0x823C, 0x84AA, 0x86D9, 0x8986, 0x8BF5, 0x8D63, 0x8F10,
         0x91AD, 0x93DE, 0x9548, 0x973B, 0x9864, 0x9A17, 0x9C81, 0x9EF2, 0xA1F8, 0xA38B, 0xA51D, 0xA76E,
         0xA831, 0xAA42, 0xACD4, 0xAEA7, 0xB01A, 0xB269, 0xB4FF, 0xB68C, 0xB9D3, 0xBBA0, 0xBD36, 0xBF45,
         0xC151, 0xC322, 0xC5B4, 0xC7C7, 0xC898, 0xCAEB, 0xCC7D, 0xCE0E, 0xD0B3, 0xD2C0, 0xD456, 0xD625,
         0xD97A, 0xDB09, 0xDD9F, 0xDFEC, 0xE0E6, 0xE295, 0xE403, 0xE670, 0xE92F, 0xEB5C, 0xEDCA, 0xEFB9,
         0xF104, 0xF377, 0xF5E1, 0xF792, 0xF8CD, 0xFABE, 0xFC28, 0xFE5B];

    readonly static byte[] BS_SOURCED_VOICE_SYNC = [0x07, 0x55, 0xFD, 0x7D, 0xF7, 0x5F, 0x70];
    readonly static byte[] BS_SOURCED_DATA_SYNC = [0x0D, 0xFF, 0x57, 0xD7, 0x5D, 0xF5, 0xD0];

    readonly static byte[] MS_SOURCED_VOICE_SYNC = [0x07, 0xF7, 0xD5, 0xDD, 0x57, 0xDF, 0xD0];
    readonly static byte[] MS_SOURCED_DATA_SYNC = [0x0D, 0x5D, 0x7F, 0x77, 0xFD, 0x75, 0x70];

    //readonly static byte[] SYNC_MASK = [0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xF0];

    // see ETSI TS 102 361-1 V2.6.1 (2023-05), B.3.12 Data Type CRC Mask
    static readonly byte[] VOICE_LC_HEADER_CRC_MASK = [0x96, 0x96, 0x96];
    static readonly byte[] TERMINATOR_WITH_LC_CRC_MASK = [0x99, 0x99, 0x99];
    static readonly byte[] PI_HEADER_CRC_MASK = [0x69, 0x69];
    static readonly byte[] DATA_HEADER_CRC_MASK = [0xCC, 0xCC];
    static readonly byte[] CSBK_CRC_MASK = [0xA5, 0xA5];


    // Internal buffer to build the DMR frame
    readonly static byte[] dmrFrame = new byte[55];

    static readonly Random _rand = new();


    /// <summary>
    /// Create a DMR Header frame.
    /// </summary>
    /// <param name="state">The current client state containing transmission parameters.</param>
    /// <param name="eot">Defines end of transmission.</param>
    /// <returns>A reference to the full DMR frame.</returns>
    public static byte[] CreateHeaderFrame(DmrSessionContext state, bool eot)
    {
        state.TxFrameType = FrameType.DataSync;

        WriteSyncPattern(FrameType.DataSync, false);

        byte[] lcData = new byte[12];
        byte[] parity = new byte[4];

        GetLcBytes(state, lcData);

        Crs129.Encode(lcData, 9, parity);

        if (eot)
        {
            lcData[9] = (byte)(parity[2] ^ TERMINATOR_WITH_LC_CRC_MASK[0]);
            lcData[10] = (byte)(parity[1] ^ TERMINATOR_WITH_LC_CRC_MASK[1]);
            lcData[11] = (byte)(parity[0] ^ TERMINATOR_WITH_LC_CRC_MASK[2]);
            state.TxStreamId = 0;
        }
        else
        {
            lcData[9] = (byte)(parity[2] ^ VOICE_LC_HEADER_CRC_MASK[0]);
            lcData[10] = (byte)(parity[1] ^ VOICE_LC_HEADER_CRC_MASK[1]);
            lcData[11] = (byte)(parity[0] ^ VOICE_LC_HEADER_CRC_MASK[2]);
            state.TxStreamId = (uint)_rand.Next(0, int.MaxValue);
            state.TxFrameCount = 0;
        }

        EncodeSlotType(state, eot ? DataType.TerminatorWithLc : DataType.DataHeader);

        DmrBptcCodec.Encode(lcData, dmrFrame, 20);

        WriteFrameHeader(state, eot ? 2 : 1);
        
        return dmrFrame;
    }

    /// <summary>
    /// Create a DMR Voice frame.
    /// </summary>
    /// <param name="state">The current client state containing transmission parameters.</param>
    /// <param name="ambe"></param>
    /// <returns>A reference to the full DMR frame.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static byte[] CreateVoiceFrame(DmrSessionContext state, byte[] ambe)
    {
        if (state.TxFrameCount <= 0)
        {
            throw new InvalidOperationException($"Invalid state: state.TxFrameCount = {state.TxFrameCount}");
        }

        // Copy AMBE data into frame
        Buffer.BlockCopy(ambe, 0, dmrFrame, 20, 13);
        dmrFrame[33] = (byte)(ambe[13] & 0xF0);
        dmrFrame[39] = (byte)(ambe[13] & 0x0F);
        Buffer.BlockCopy(ambe, 14, dmrFrame, 40, 13);

        int seqNo = (state.TxFrameCount - 1) % 6;
        if (seqNo == 0)
        {
            state.TxFrameType = FrameType.VoiceSync;
            WriteSyncPattern(FrameType.VoiceSync, false);
            EncodeEmbeddedData(state);
        }
        else
        {
            state.TxFrameType = FrameType.Voice;
            byte lcss = WriteEmbeddedData(seqNo);
            EncodeEmbeddedSignaling(state.TxColorCode, lcss);
        }

        WriteFrameHeader(state, seqNo);
        return dmrFrame;
    }

    /// <summary>
    /// Builds and finalizes the 20-byte DMR frame header, encoding source/destination IDs, repeater ID, slot, FLCO, frame type, 
    /// and stream ID (for the first frame).
    /// </summary>
    /// <param name="clientState">The current client state containing required transmission parameters.</param>
    /// <param name="seqNo">The sequence number of the frame.</param>
    /// <exception cref="NotImplementedException">Thrown if the Homebrew protocol is selected (not implemented).</exception>
    private static void WriteFrameHeader(DmrSessionContext clientState, int seqNo)
    {
        Encoding.ASCII.GetBytes("DMRD").CopyTo(dmrFrame, 0);

        dmrFrame[4] = (byte)clientState.TxFrameCount;

        dmrFrame[5] = (byte)(clientState.TxSrcId >> 16);
        dmrFrame[6] = (byte)(clientState.TxSrcId >> 8);
        dmrFrame[7] = (byte)(clientState.TxSrcId >> 0);

        dmrFrame[8] = (byte)(clientState.TxDstId >> 16);
        dmrFrame[9] = (byte)(clientState.TxDstId >> 8);
        dmrFrame[10] = (byte)(clientState.TxDstId >> 0);

        dmrFrame[11] = (byte)(clientState.TxRptId >> 24); // TODO: MyDmrId + Essid
        dmrFrame[12] = (byte)(clientState.TxRptId >> 16);
        dmrFrame[13] = (byte)(clientState.TxRptId >> 8);
        dmrFrame[14] = (byte)(clientState.TxRptId >> 0);

        switch (clientState.Protocol)
        {
            case DmrProtocol.Homebrew:
                dmrFrame[15] = (byte)(clientState.TxTimeSlot == 2 ? 1 : 0);
                dmrFrame[15] |= (byte)((clientState.TxFlco == Flco.USER_USER ? 1 : 0) << 1);
                dmrFrame[15] |= (byte)(((int)clientState.TxFrameType & 0x03) << 2);
                if (clientState.TxFrameType is FrameType.DataSync or FrameType.Voice) {
                    dmrFrame[15] |= (byte)((seqNo & 0x0F) << 4);
                }
                break; 
            case DmrProtocol.MmdvmHost:
                dmrFrame[15] = (byte)((clientState.TxTimeSlot == 1) ? 0x00 : 0x80);
                dmrFrame[15] |= (byte)((clientState.TxFlco == Flco.GROUP) ? 0x00 : 0x40);
                dmrFrame[15] |= (byte)(((byte)clientState.TxFrameType) << 4);
                if (clientState.TxFrameType is FrameType.DataSync or FrameType.Voice) 
                {
                    dmrFrame[15] |= (byte)(seqNo & 0x0F);
                }
                break;
        }

        // StreamId as BE
        dmrFrame[16] = (byte)(clientState.TxStreamId >> 24);
        dmrFrame[17] = (byte)(clientState.TxStreamId >> 16);
        dmrFrame[18] = (byte)(clientState.TxStreamId >> 8);
        dmrFrame[19] = (byte)(clientState.TxStreamId);
    }

    internal static byte[] DecodeDmrFrame(DmrSessionContext clientState, byte[] dmrPkt)
    {
        clientState.RxFrameNo = dmrPkt[4];
        clientState.RxSrcId = (dmrPkt[5] << 16) | (dmrPkt[6] << 8) | dmrPkt[7]; // 24 bits BE -> LE
        clientState.RxDstId = (dmrPkt[8] << 16) | (dmrPkt[9] << 8) | dmrPkt[10]; // 24 bits BE -> LE
        clientState.RxRptId = (dmrPkt[11] << 24) | (dmrPkt[12] << 16) | (dmrPkt[13] << 8) | dmrPkt[14]; // 32 bits BE -> LE

        // DG1YIQ (Marco Reinke):
        // Bits 0 to 4 are clear - Bit 0->Slot Np, Bit 1->Call Type, Bit 2 - 3 -> Frame Type...
        // Bits 4 - 7 are the Voice Sequence(for Voice) ->Voice is Transmitted in so called Superframes, these consist of 6 DMR Voice Frames A to F.

        // It is not AMBE alone...it is the whole DMR Frame of 264 Bit(33 byte)
        // One DMR Frame contains 3 times AMBE Frames (including FEC) for each 20ms Audio and also SYNC or Embedded Information (48bit)
        // So a DMR Frame contains in total 60ms Audio(3 times AMBE Frame 20ms) -one AMBE Frame are 49bit Codec +23 bit FEC = 72 bit(9 Bytes) -
        // makes Totals in an DMr Frame 216 bit Payload
        // Payload 216 bit + 48 bit Sync or Embedded makes total 264 Bit (33 Bytes)

        byte flagBits = dmrPkt[15];
        switch (clientState.Protocol)
        {
            case DmrProtocol.Homebrew:
                clientState.RxTimeSlot = (flagBits & 0b00000001) == 0 ? 1 : 2;                    // Bit 0
                clientState.RxFlco = (flagBits & 0b00000010) == 0 ? Flco.GROUP : Flco.USER_USER; // Bit 1
                clientState.RxFrameType = (FrameType)((flagBits & 0b00001100) >> 2);    // Bits 2-3
                clientState.RxVoiceOrDataSeq = (flagBits & 0xF0) >> 4;                    // Bits 4-7
                break;
            case DmrProtocol.MmdvmHost:
                clientState.RxTimeSlot = (flagBits & 0x80) != 0 ? 2 : 1;
                clientState.RxFlco = (flagBits & 0x40) == 0 ? Flco.GROUP : Flco.USER_USER;
                clientState.RxFrameType = (FrameType)((flagBits & 0x30) >> 4);
                clientState.RxVoiceOrDataSeq = (flagBits & 0x0F);
                break;
        }

        clientState.RxStreamId = (uint)((dmrPkt[16] << 24) | (dmrPkt[17] << 16) | (dmrPkt[18] << 8) | dmrPkt[19]); // 32 bits BE -> LE

        logger.Debug($"frameNo={clientState.RxFrameNo}, srcId={clientState.RxSrcId}, dstId={clientState.RxDstId}, rptId={clientState.RxRptId}, slot={clientState.RxTimeSlot}, streamId={clientState.RxStreamId}, {clientState.RxFlco}, {clientState.RxFrameType}, Flag Bits={Convert.ToString(flagBits, 2).PadLeft(8, '0')}");

        byte[] ambeBuffer = new byte[3 * 9];
        if (clientState.RxFrameType is FrameType.Voice or FrameType.VoiceSync)
        {
            Buffer.BlockCopy(dmrPkt, 20, ambeBuffer, 0, 14); // Copy first 14 bytes directly from dmrPkt starting at offset 20
            ambeBuffer[13] = (byte)((ambeBuffer[13] & 0xF0) | (dmrPkt[39] & 0x0F));  // Patch byte 13: high nibble stays, low nibble is taken from dmrPkt[39] (offset 20 + 19)
            Buffer.BlockCopy(dmrPkt, 40, ambeBuffer, 14, 13); // Copy final 13 bytes from dmrPkt starting at offset 40 (20 + 20)
        }

        return ambeBuffer;
    }

    /// <summary>
    /// Decodes the DMR Slot Type PDU from the current <see cref="dmrFrame"/> and extracts the Color Code and Data Type.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <list type="number">
    /// <item><description><c>int</c> - the Color Code (upper 4 bits of the slot type byte)</description></item>
    /// <item><description><see cref="DataType"/> - the decoded frame type (lower 4 bits of the slot type byte)</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method reconstructs the 3-byte Slot Type block from the packed bitfields located in the DMR frame payload (bytes at <c>offset+12</c>,
    /// <c>offset+13</c>, <c>offset+19</c>, and <c>offset+20</c>). The block is then passed through <see cref="Golay2087.Decode(byte[])"/>
    /// to apply Golay(20,8) FEC decoding. The resulting first byte is split into:
    /// <list type="bullet">
    /// <item>Upper 4 bits -> Color Code</item>
    /// <item>Lower 4 bits -> <see cref="DataType"/></item>
    /// </list>
    /// 
    /// Reference: ETSI TS 102 361-1 V2.6.1 (2023-05), 9.1.3 "Slot Type (SLOT) PDU".
    /// </remarks>
    public static (int, DataType) DecodeSlotType()
    {
        const int offset = 20;
        byte[] dmrSlotType =
        [
            // Reconstruct the 3 input bytes from the packed bits
            (byte)(((dmrFrame[offset + 12] & 0x3F) << 2) | ((dmrFrame[offset + 13] & 0xC0) >> 6)),
            (byte)(((dmrFrame[offset + 13] & 0x30) << 2) | ((dmrFrame[offset + 19] & 0x0F) << 2) | ((dmrFrame[offset + 20] & 0xC0) >> 6)),
            (byte)(((dmrFrame[offset + 20] & 0x3C) << 2)),
        ];
        byte b = Golay2087.Decode(dmrSlotType);
        return (b >> 4, (DataType)(b & 0x0F));
    }

    /// <summary>
    /// Encodes the Slot Type PDU for a DMR frame and writes the packed result
    /// into the correct bit fields of the internal buffer <see cref="dmrFrame"/>.
    /// </summary>
    /// <param name="state">The current client state providing the Color Code and other slot parameters.</param>
    /// <param name="dataType">The <see cref="DataType"/> value representing the current frame type (e.g. voice, data, control).
    /// </param>
    /// <remarks>
    /// The method builds the 3-byte Slot Type field as follows:
    /// <list type="bullet">
    /// <item>[0] = Color Code (upper 4 bits) + Frame Type (lower 4 bits)</item>
    /// <item>[1] = Reserved / slot-specific bits (initially 0)</item>
    /// <item>[2] = Reserved / slot-specific bits (initially 0)</item>
    /// </list>
    /// This 3-byte block is then Golay(20,8) encoded for FEC protection,
    /// and the resulting 20 bits are scattered across <paramref name="dmrFrame"/>
    /// at positions <c>offset+12</c>, <c>offset+13</c>, <c>offset+19</c>, and <c>offset+20</c>.
    /// <para>
    /// Reference: ETSI TS 102 361-1 V2.6.1 (2023-05), 9.1.3 "Slot Type (SLOT) PDU".
    /// </para>
    /// </remarks>
    private static void EncodeSlotType(DmrSessionContext state, DataType dataType)
    {
        byte[] dmrSlotType = [(byte)((state.TxColorCode << 4) & 0xF0), 0x00, 0x00];
        dmrSlotType[0] |= (byte)(((byte)dataType) & 0x0F);

        Golay2087.Encode(dmrSlotType);

        const int offset = 20; // offset payload
        dmrFrame[offset + 12] = (byte)((dmrFrame[offset + 12] & 0xC0) | ((dmrSlotType[0] >> 2) & 0x3F));
        dmrFrame[offset + 13] = (byte)((dmrFrame[offset + 13] & 0x0F) | ((dmrSlotType[0] << 6) & 0xC0) | ((dmrSlotType[1] >> 2) & 0x30));
        dmrFrame[offset + 19] = (byte)((dmrFrame[offset + 19] & 0xF0) | ((dmrSlotType[1] >> 2) & 0x0F));
        dmrFrame[offset + 20] = (byte)((dmrFrame[offset + 20] & 0x03) | ((dmrSlotType[1] << 6) & 0xC0) | ((dmrSlotType[2] >> 2) & 0x3C));
    }

    static readonly bool[] tmpBuffer72Bits = new bool[72]; 
    static readonly bool[] tmpBuffer128Bits = new bool[128];
    static readonly bool[] encodedLcBits = new bool[128];

    /// <summary>
    /// Encode the Link Control embedded data with CRC and Hamming (16,11,4) FEC, then pack it column-wise into a 128-bit buffer: <see cref="encodedLcBits"/>.
    /// This buffer is used in DMR voice bursts for transmission.
    /// </summary>
    public static void EncodeEmbeddedData(DmrSessionContext state)
    {
        // Compute 5-bit CRC over tmpBuffer72Bits
        GetLcBits(state, tmpBuffer72Bits);
        uint crc = Crc.EncodeFiveBit(tmpBuffer72Bits);

        Array.Fill(tmpBuffer128Bits, false);

        // Place CRC bits in designated positions
        tmpBuffer128Bits[106] = (crc & 0x01) != 0;
        tmpBuffer128Bits[90] = (crc & 0x02) != 0;
        tmpBuffer128Bits[74] = (crc & 0x04) != 0;
        tmpBuffer128Bits[58] = (crc & 0x08) != 0;
        tmpBuffer128Bits[42] = (crc & 0x10) != 0;

        // Copy the LC data into the appropriate positions (skipping parity bits)
        //int b = 0;
        //for (int a = 0; a < 11; a++, b++) tmpBuffer128Bits[a] = tmpBuffer72Bits[b];
        //for (int a = 16; a < 27; a++, b++) tmpBuffer128Bits[a] = tmpBuffer72Bits[b];
        //for (int a = 32; a < 42; a++, b++) tmpBuffer128Bits[a] = tmpBuffer72Bits[b];
        //for (int a = 48; a < 58; a++, b++) tmpBuffer128Bits[a] = tmpBuffer72Bits[b];
        //for (int a = 64; a < 74; a++, b++) tmpBuffer128Bits[a] = tmpBuffer72Bits[b];
        //for (int a = 80; a < 90; a++, b++) tmpBuffer128Bits[a] = tmpBuffer72Bits[b];
        //for (int a = 96; a < 106; a++, b++) tmpBuffer128Bits[a] = tmpBuffer72Bits[b];
        int b = 0; // source index
        for (int segment = 0; segment < 7; segment++)
        {
            int start = segment * 16;
            int length = (segment < 2) ? 11 : 10; // first two segments 11 bits, rest 10 bits

            for (int a = start; a < start + length; a++, b++)
            {
                tmpBuffer128Bits[a] = tmpBuffer72Bits[b];
            }
        }

        // Apply Hamming (16,11,4) FEC on each row except last one
        for (int a = 0; a < 112; a += 16)
        {
            Encode16114(tmpBuffer128Bits, a);
        }

        // Add the column parity bits
        for (int a = 0; a < 16; a++)
        {
            tmpBuffer128Bits[a + 112] = tmpBuffer128Bits[a] ^ tmpBuffer128Bits[a + 16] ^ tmpBuffer128Bits[a + 32] ^ tmpBuffer128Bits[a + 48] ^
                            tmpBuffer128Bits[a + 64] ^ tmpBuffer128Bits[a + 80] ^ tmpBuffer128Bits[a + 96];
        }

        // Pack the data column-wise into encodedLcBits
        b = 0;
        for (int a = 0; a < 128; a++)
        {
            encodedLcBits[a] = tmpBuffer128Bits[b];
            b += 16;
            if (b > 127) b -= 127;
        }
    }


    /// <summary>
    /// Encodes the 16-bit embedded signaling (EI) data for a DMR frame using the DMR client's current state (Color Code) and a 2-bit LCSS value, applies
    /// QR(16,7,6) error protection, and inserts the resulting nibbles into the standard EI positions in the internal frame buffer <see cref="dmrFrame"/>.
    /// </summary>
    /// <param name="colorCode">The color code to be considered.</param>
    /// <param name="lcss">The 2-bit Link Control Start/Stop to encode.</param>
    /// <remarks>
    /// <para>
    /// Reference: ETSI TS 102 361-1 V2.6.1 (2023-05), 9.1.2 Embedded signalling (EMB) PDU.
    /// </para>
    /// </remarks>
    private static void EncodeEmbeddedSignaling(int colorCode, byte lcss)
    {
        byte[] emb = new byte[2];
        emb[0] = (byte)((colorCode << 4) & 0xF0);
        emb[0] |= (byte)((lcss << 1) & 0x06);

        EncodeQuadraticResidue1676(emb); // QR(16,7,6) error protection

        // Pack 16 bits across 4 nibbles in the DMR frame
        const int offset = 20;
        dmrFrame[offset + 13] = (byte)((dmrFrame[offset + 13] & 0xF0) | ((emb[0] >> 4) & 0x0F));
        dmrFrame[offset + 14] = (byte)((dmrFrame[offset + 14] & 0x0F) | ((emb[0] << 4) & 0xF0));
        dmrFrame[offset + 18] = (byte)((dmrFrame[offset + 18] & 0xF0) | ((emb[1] >> 4) & 0x0F));
        dmrFrame[offset + 19] = (byte)((dmrFrame[offset + 19] & 0x0F) | ((emb[1] << 4) & 0xF0));
    }

    /// <summary>
    /// Extracts embedded signaling data that are created by method <see cref="EncodeEmbeddedData(DmrSessionContext)"/> from 
    /// the internal buffer <see cref="encodedLcBits"/> and writes it into the internal frame buffer <see cref="dmrFrame"/>.
    /// </summary>
    /// <param name="n">Index of the embedded data block (1..4).</param>
    /// <returns>
    /// 0 = no data / invalid index, 
    /// 1 = first block, 
    /// 2 = fourth block, 
    /// 3 = blocks 2 or 3
    /// </returns>
    static byte WriteEmbeddedData(int n)
    {
        const int offset = 20; // offset payload in DMR frame buffer
        
        if (n >= 1 && n < 5)
        {
            n--; // adjust index to 0-based

            bool[] bits = new bool[40]; // initialize 40-bit temp buffer
            Buffer.BlockCopy(encodedLcBits, n * 32, bits, 4, 32); // copy 32 bits into bits

            byte[] ba =
            [
                BitsToByteBE(bits, 0),
                BitsToByteBE(bits, 8),
                BitsToByteBE(bits, 16),
                BitsToByteBE(bits, 24),
                BitsToByteBE(bits, 32),
            ];

            dmrFrame[offset + 14] = (byte)((dmrFrame[offset + 14] & 0xF0) | (ba[0] & 0x0F));
            dmrFrame[offset + 15] = ba[1];
            dmrFrame[offset + 16] = ba[2];
            dmrFrame[offset + 17] = ba[3];
            dmrFrame[offset + 18] = (byte)((dmrFrame[offset + 18] & 0x0F) | (ba[4] & 0xF0));

            return n switch
            {
                0 => 1,
                3 => 2,
                _ => 3
            };
        }
        else
        {
            dmrFrame[offset + 14] &= 0xF0;
            dmrFrame[offset + 15] = 0x00;
            dmrFrame[offset + 16] = 0x00;
            dmrFrame[offset + 17] = 0x00;
            dmrFrame[offset + 18] &= 0x0F;

            return 0;
        }
    }


    /// <summary>
    /// Computes and sets the 5 Hamming parity bits for an 16-bit block using the (16,11,4) Hamming code,
    /// starting at the specified offset in the boolean array.
    /// The first 11 bits at the offset are data bits, and the last 5 bits (offset+11..offset+15) are parity bits.
    /// </summary>
    /// <param name="d">A boolean array containing at least offset + 16 elements.</param>
    /// <param name="offset">The starting index in the array where the 16-bit block begins.</param>
    /// <exception cref="ArgumentException">Thrown if the array is null or not large enough for the offset + 16 bits.</exception>
    public static void Encode16114(bool[] d, int offset)
    {
        if (d == null || d.Length < offset + 16)
            throw new ArgumentException("Array must contain at least 16 elements from the given offset.");

        d[offset + 11] = d[offset + 0] ^ d[offset + 1] ^ d[offset + 2] ^ d[offset + 3] ^ d[offset + 5] ^ d[offset + 7] ^ d[offset + 8];
        d[offset + 12] = d[offset + 1] ^ d[offset + 2] ^ d[offset + 3] ^ d[offset + 4] ^ d[offset + 6] ^ d[offset + 8] ^ d[offset + 9];
        d[offset + 13] = d[offset + 2] ^ d[offset + 3] ^ d[offset + 4] ^ d[offset + 5] ^ d[offset + 7] ^ d[offset + 9] ^ d[offset + 10];
        d[offset + 14] = d[offset + 0] ^ d[offset + 1] ^ d[offset + 2] ^ d[offset + 4] ^ d[offset + 6] ^ d[offset + 7] ^ d[offset + 10];
        d[offset + 15] = d[offset + 0] ^ d[offset + 2] ^ d[offset + 5] ^ d[offset + 6] ^ d[offset + 8] ^ d[offset + 9] ^ d[offset + 10];
    }


    /// <summary>
    /// Encodes 7 information bits into a 16-bit QR(16,7,6) codeword.
    /// 
    /// Input:
    /// - data[0]: contains 7 information bits in its upper 7 bits (bits 1..7).
    /// 
    /// Output:
    /// - data[0]: high byte of 16-bit codeword
    /// - data[1]: low byte of 16-bit codeword
    /// 
    /// Reference: ETSI TS 102 361-1, Quadratic Residue code QR(16,7,6).
    /// </summary>
    /// <param name="data">Array with at least 2 bytes. The encoded codeword is written back into indices 0 and 1.</param>
    internal static void EncodeQuadraticResidue1676(byte[] data)
    {
        int value = (data[0] >> 1) & 0x7F;
        int cksum = EncodingTable1676[value];

        data[0] = (byte)(cksum >> 8);
        data[1] = (byte)(cksum & 0xFF);
    }

    /// <summary>
    /// Fills an array of bits with LC (Link Control) data, extracted from LC bytes.
    /// </summary>
    /// <param name="state">The DMR client state contains the relevent source data.</param>
    /// <param name="bits">The output bit array.</param>
    public static void GetLcBits(DmrSessionContext state, bool[] bits)
    {
        if (bits == null || bits.Length < 72)
            throw new ArgumentException("bits must have a length of at least 72.", nameof(bits));

        byte[] lcData = new byte[9];
        GetLcBytes(state, lcData);

        for (int i = 0; i < lcData.Length; i++)
        {
            var tmp = new bool[8];
            ByteToBitsBE(lcData[i], tmp);
            tmp.CopyTo(bits, i * 8);
        }
    }

    /// <summary>
    /// Builds LC (Link Control) data as a 9-byte array.
    /// See: ETSI TS 102 361-2 V2.5.1 (2023-05), chap. 7.1.1
    /// </summary>
    /// <param name="state">Input data</param>
    /// <param name="outBuffer">The byte array to write the result (min. length = 9).</param>
    public static void GetLcBytes(DmrSessionContext state, byte[] outBuffer)
    {
        if (outBuffer == null || outBuffer.Length < 9)
            throw new ArgumentException("bytes must contain at least 9 elements.", nameof(outBuffer));

        outBuffer[0] = (byte)state.TxFlco;
        outBuffer[1] = 0; // FID
        outBuffer[2] = 0; // options
        outBuffer[3] = (byte)(state.TxDstId >> 16);
        outBuffer[4] = (byte)(state.TxDstId >> 8);
        outBuffer[5] = (byte)(state.TxDstId);
        outBuffer[6] = (byte)(state.TxSrcId >> 16);
        outBuffer[7] = (byte)(state.TxSrcId >> 8);
        outBuffer[8] = (byte)(state.TxSrcId);
    }


    /// <summary>
    /// Extracts the 6-byte Embedded Identifier (EI) from a DMR frame payload.
    /// </summary>
    /// <param name="payload">The full DMR frame payload (at least 33 bytes from the specified offset).</param>
    /// <param name="offset">The starting index within <paramref name="payload"/> where the 33-byte frame begins.</param>
    /// <param name="eiOut">A 6-byte array that will receive the extracted EI bytes.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="payload"/> or <paramref name="eiOut"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="payload"/> does not contain at least 33 bytes from <paramref name="offset"/>,
    /// or if <paramref name="eiOut"/> has fewer than 6 bytes.
    /// </exception>
    /// <remarks>
    /// The extraction process takes the lower nibble of the first byte, combines it with the upper nibble 
    /// of the next byte, and continues this pattern across the payload bytes:
    /// <list type="bullet">
    ///   <item>eiOut[0] = low nibble of payload[offset+13] + high nibble of payload[offset+14]</item>
    ///   <item>eiOut[1] = low nibble of payload[offset+14] + high nibble of payload[offset+15]</item>
    ///   <item>eiOut[2] = low nibble of payload[offset+15] + high nibble of payload[offset+16]</item>
    ///   <item>eiOut[3] = low nibble of payload[offset+16] + high nibble of payload[offset+17]</item>
    ///   <item>eiOut[4] = low nibble of payload[offset+17] + high nibble of payload[offset+18]</item>
    ///   <item>eiOut[5] = low nibble of payload[offset+18] + high nibble of payload[offset+19]</item>
    /// </list>
    /// This corresponds to the DMR embedded signaling extraction for blocks B–F in the voice frame.
    /// </remarks>
    public static void ExtractEiFromFrame(byte[] payload, int offset, byte[] eiOut)
    {
        if (payload == null || payload.Length < offset + 33) throw new ArgumentException($"{payload} must contain at least 33 bytes from offset");
        if (eiOut == null || eiOut.Length < 6) throw new ArgumentException($"{eiOut} must be 6 bytes");

        // low nibble of frame[offset + 13], then frame[offset + 14..18], then high nibble of frame[offset + 19].
        eiOut[0] = (byte)(((payload[offset + 13] & 0x0F) << 4) | (payload[offset + 14] >> 4));
        eiOut[1] = (byte)(((payload[offset + 14] & 0x0F) << 4) | (payload[offset + 15] >> 4));
        eiOut[2] = (byte)(((payload[offset + 15] & 0x0F) << 4) | (payload[offset + 16] >> 4));
        eiOut[3] = (byte)(((payload[offset + 16] & 0x0F) << 4) | (payload[offset + 17] >> 4));
        eiOut[4] = (byte)(((payload[offset + 17] & 0x0F) << 4) | (payload[offset + 18] >> 4));
        eiOut[5] = (byte)(((payload[offset + 18] & 0x0F) << 4) | (payload[offset + 19] >> 4));
    }

    /// <summary>
    /// Converts 8 boolean values (most significant bit first) into a single byte.
    /// </summary>
    /// <param name="bits">Array of at least 8 booleans representing bit values.</param>
    /// <returns>A byte with the corresponding bit values packed in big-endian order.</returns>
    public static byte BitsToByteBE(bool[] bits)
    {
        if (bits == null || bits.Length < 8)
            throw new ArgumentException("bits must contain at least 8 elements.", nameof(bits));

        byte result = 0x00;
        result |= (byte)(bits[0] ? 0x80 : 0x00);
        result |= (byte)(bits[1] ? 0x40 : 0x00);
        result |= (byte)(bits[2] ? 0x20 : 0x00);
        result |= (byte)(bits[3] ? 0x10 : 0x00);
        result |= (byte)(bits[4] ? 0x08 : 0x00);
        result |= (byte)(bits[5] ? 0x04 : 0x00);
        result |= (byte)(bits[6] ? 0x02 : 0x00);
        result |= (byte)(bits[7] ? 0x01 : 0x00);
        return result;
    }

    /// <summary>
    /// Converts 8 bits from a bool array (big-endian) to a byte.
    /// </summary>
    public static byte BitsToByteBE(bool[] bits, int offset)
    {
        byte result = 0x00;
        for (int i = 0; i < 8; i++)
        {
            result <<= 1;
            if (bits[offset + i])
                result |= 1;
        }
        return result;
    }

    /// <summary>
    /// Converts a byte into 8 boolean values (most significant bit first).
    /// </summary>
    /// <param name="value">The byte to convert.</param>
    /// <param name="bits">Output array of length 8 that will receive the bit values.</param>
    public static void ByteToBitsBE(byte value, bool[] bits)
    {
        if (bits == null || bits.Length < 8)
            throw new ArgumentException($"Argument bits must contain at least 8 elements.", nameof(bits));

        bits[0] = (value & 0x80) == 0x80;
        bits[1] = (value & 0x40) == 0x40;
        bits[2] = (value & 0x20) == 0x20;
        bits[3] = (value & 0x10) == 0x10;
        bits[4] = (value & 0x08) == 0x08;
        bits[5] = (value & 0x04) == 0x04;
        bits[6] = (value & 0x02) == 0x02;
        bits[7] = (value & 0x01) == 0x01;
    }

    /// <summary>
    /// Inserts the appropriate DMR SYNC pattern into the static DMR frame at a fixed offset.
    /// <para> 
    /// See also: ETSI TS 102 361-1 V2.6.1 (2023-05), 9.1.1 Synchronization (SYNC) PDU, Table 9.2: SYNC patterns
    /// </para>
    /// </summary>
    /// <param name="frameType">The type of frame to synchronize: DataSync or VoiceSync.</param>
    /// <param name="baseStation">True for Base Station, false for Mobile Station. Determines which SYNC pattern is used.</param>
    /// <exception cref="ArgumentException">Thrown if an invalid frameType is provided.</exception>
    /// <remarks>
    /// Writes 7 bytes of the corresponding SYNC pattern into the static dmrFrame array,
    /// using SYNC_MASK to preserve unrelated bits. The fixed offset is 33 (20 + 13).
    /// </remarks>
    public static void WriteSyncPattern(FrameType frameType, bool baseStation)
    {
        const int offset = 20 + 13;
        byte[] syncPattern = frameType switch
        {
            FrameType.DataSync => baseStation ? BS_SOURCED_DATA_SYNC : MS_SOURCED_DATA_SYNC,
            FrameType.VoiceSync => baseStation ? BS_SOURCED_VOICE_SYNC : MS_SOURCED_VOICE_SYNC,
            _ => throw new ArgumentException($"Invalid argument {nameof(frameType)} = {frameType}"),
        };

        // First byte: preserve high nibble
        dmrFrame[offset + 0] = (byte)((dmrFrame[offset + 0] & 0xF0) | (syncPattern[0] & 0x0F));

        // Middle bytes: fully overwrite
        for (int i = 1; i < 6; i++)
        {
            dmrFrame[offset + i] = syncPattern[i];
        }

        // Last byte: preserve low nibble
        dmrFrame[offset + 6] = (byte)((dmrFrame[offset + 6] & 0x0F) | (syncPattern[6] & 0xF0));
    }
    //public static void WriteSyncPattern2(FrameType frameType, bool duplex)
    //{
    //    const int offset = 20 + 13;

    //    switch (frameType)
    //    {
    //        case FrameType.DataSync:
    //            for (int i = 0; i < 7; i++)
    //            {
    //                dmrFrame[offset + i] = (byte)((dmrFrame[offset + i] & ~SYNC_MASK[i]) | (duplex ? BS_SOURCED_DATA_SYNC[i] : MS_SOURCED_DATA_SYNC[i]));
    //            }
    //            break;
    //        case FrameType.VoiceSync:
    //            for (int i = 0; i < 7; i++)
    //            {
    //                dmrFrame[offset + i] = (byte)((dmrFrame[offset + i] & ~SYNC_MASK[i]) | (duplex ? BS_SOURCED_VOICE_SYNC[i] : MS_SOURCED_VOICE_SYNC[i]));
    //            }
    //            break;
    //        default:
    //            throw new ArgumentException($"Invalid argument {nameof(frameType)} = {frameType}");
    //    }
    //}

    // TA stuf

    /// <summary>
    ///  Extracts the 6-byte Embedded Identifier (EI) from a DMR frame payload.
    //Args:
    //        payload(bytes | bytearray) : The full DMR frame payload(must contain at least 33 bytes from offset).
    //        offset(int) : The starting index within payload where the 33-byte frame begins.
    //        ei_out(bytearray): A 6-byte array that will receive the extracted EI bytes.
    /// </summary>
    /// <param name="payload">The full DMR frame payload(must contain at least 33 bytes from offset).</param>
    /// <param name="eiOut"></param>
    /// <exception cref="ArgumentException"></exception>
    static void ExtractEiFromFrame(ReadOnlySpan<byte> payload, Span<byte> eiOut)
    {
        if (payload.Length < 33) throw new ArgumentException("frame must be 33 bytes");
        if (eiOut.Length < 6) throw new ArgumentException("eiOut must be 6 bytes");

        // See explanation earlier: low nibble of f[13], then f[14]..f[18], then high nibble of f[19].
        eiOut[0] = (byte)(((payload[13] & 0x0F) << 4) | (payload[14] >> 4));
        eiOut[1] = (byte)(((payload[14] & 0x0F) << 4) | (payload[15] >> 4));
        eiOut[2] = (byte)(((payload[15] & 0x0F) << 4) | (payload[16] >> 4));
        eiOut[3] = (byte)(((payload[16] & 0x0F) << 4) | (payload[17] >> 4));
        eiOut[4] = (byte)(((payload[17] & 0x0F) << 4) | (payload[18] >> 4));
        eiOut[5] = (byte)(((payload[18] & 0x0F) << 4) | (payload[19] >> 4));
    }

}