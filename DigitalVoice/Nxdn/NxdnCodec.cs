using NLog;
using System.Text;
using static DigitalVoice.Nxdn.BitOperations;

namespace DigitalVoice.Nxdn;

/// <summary>
/// Class used to decode/encode NXDND packet data.
/// </summary>
internal static class NxdnCodec
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static readonly int[] dvsiInterleave = [
        0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 41, 43, 45, 47,
        1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34, 37, 40, 42, 44, 46, 48,
        2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35, 38
    ];

    private const byte NXDN_MESSAGE_TYPE_VCALL = 1;
    private const byte NXDN_MESSAGE_TYPE_TX_REL = 8;

    private const byte NXDN_LICH_RFCT_RDCH = 2;
    private const byte NXDN_LICH_USC_SACCH_NS = 0;
    private const byte NXDN_LICH_USC_SACCH_SS = 2;
    private const byte NXDN_LICH_STEAL_FACCH = 0;
    private const byte NXDN_LICH_STEAL_NONE = 3;
    private const byte NXDN_LICH_DIRECTION_INBOUND = 0;

    private static readonly byte[] _nxdnFrame = new byte[43];
    private static readonly byte[] _sacch = new byte[5];
    private static readonly byte[] _layer3 = new byte[22];

    private static readonly byte[] idle = { 0x10, 0x00, 0x00 };
    private static byte _lich;


    /// <summary>
    /// Decode a 43 bytes NXDND frame.
    /// </summary>
    /// <param name="sessionContext">Contains information about the current NXDN session.</param>
    /// <param name="packet">The NXDND packet to decode (Length=43 bytes)</param>
    internal static void Decode(NxdnSessionContext sessionContext, byte[] packet)
    {
        if (packet == null || packet.Length != 43)
            throw new ArgumentException("Argument must be exactly 43 bytes", nameof(packet));

        // Reset
        sessionContext.RxAmbeData.Clear();
        sessionContext.Eot = false;

        sessionContext.Lich = NxdnLich.Parse(packet[10]);

        sessionContext.SrcId = ((packet[5] << 8) & 0xFF00) | (packet[6] & 0xFF);
        sessionContext.DstId = ((packet[7] << 8) & 0xFF00) | (packet[8] & 0xFF);

        sessionContext.Eot = (sessionContext.Lich.Usc == NxdnUsc.SacchNs) && ((packet[9] & 0x08) == 0x08);

        // AMBE Block-1
        byte[] ambeBlock = new byte[7];
        Buffer.BlockCopy(packet, 15, ambeBlock, 0, ambeBlock.Length);
        Interleave(ambeBlock);
        sessionContext.RxAmbeData.Add((byte[])ambeBlock.Clone());

        // AMBE Block-2
        for (int i = 0; i < 6; i++)
        {
            ambeBlock[i] = (byte)((packet[21 + i] << 1));
            ambeBlock[i] |= (byte)(1 & (packet[21 + i + 1] >> 7));
        }
        ambeBlock[6] = (byte)(packet[21 + 6] << 1);
        Interleave(ambeBlock);
        sessionContext.RxAmbeData.Add((byte[])ambeBlock.Clone());

        // AMBE Block-3
        Buffer.BlockCopy(packet, 29, ambeBlock, 0, ambeBlock.Length);
        Interleave(ambeBlock);
        sessionContext.RxAmbeData.Add((byte[])ambeBlock.Clone());

        // AMBE Block-4
        for (int i = 0; i < 6; i++)
        {
            ambeBlock[i] = (byte)((packet[35 + i] << 1));
            ambeBlock[i] |= (byte)(1 & (packet[35 + i + 1] >> 7));
        }
        ambeBlock[6] = (byte)(packet[35 + 6] << 1);
        Interleave(ambeBlock);
        sessionContext.RxAmbeData.Add((byte[])ambeBlock.Clone());
    }


    /// <summary>
    /// Encode a NXDN frame (header/data). This method is the entry point for NXDN frame encoding.
    /// </summary>
    /// <param name="sessionCtx">Information about the current NXDN session</param>
    /// <returns></returns>
    internal static byte[] EncodeFrame(NxdnSessionContext sessionCtx)
    {
        // Reset
        Array.Clear(_sacch, 0, _sacch.Length);
        Array.Clear(_layer3, 0, _layer3.Length);
        Array.Clear(_nxdnFrame, 0, _nxdnFrame.Length);
        _lich = 0x00;

        Encoding.ASCII.GetBytes("NXDND").CopyTo(_nxdnFrame, 0);
        _nxdnFrame[5] = (byte)((sessionCtx.NxdnId >> 8) & 0xFF);
        _nxdnFrame[6] = (byte)(sessionCtx.NxdnId & 0xFF);
        _nxdnFrame[7] = (byte)((sessionCtx.GwId >> 8) & 0xFF);
        _nxdnFrame[8] = (byte)(sessionCtx.GwId & 0xFF);
        _nxdnFrame[9] = 0x01;

        if (sessionCtx.Eot || sessionCtx.TxFrameNumber == 0)
        {
            EncodeHeader(sessionCtx);
        }
        else
        {
            EncodeData(sessionCtx);
        }

        if (_nxdnFrame[10] == 0x81 || _nxdnFrame[10] == 0x83)
        {
            _nxdnFrame[9] |= (byte)(_nxdnFrame[15] == 0x01 ? 0x04 : 0x00);
            _nxdnFrame[9] |= (byte)(_nxdnFrame[15] == 0x08 ? 0x08 : 0x00);
        }
        else if ((_nxdnFrame[10] & 0xF0) == 0x90)
        {
            _nxdnFrame[9] |= 0x02;
            if (_nxdnFrame[10] == 0x90 || _nxdnFrame[10] == 0x92 || _nxdnFrame[10] == 0x9C || _nxdnFrame[10] == 0x9E)
            {
                _nxdnFrame[9] |= (byte)(_nxdnFrame[12] == 0x09 ? 0x04 : 0x00);
                _nxdnFrame[9] |= (byte)(_nxdnFrame[12] == 0x08 ? 0x08 : 0x00);
            }
        }
       
        return _nxdnFrame;
    }


    /// <summary>
    /// Encodes a NXDN data frame incl. AMBE data.
    /// </summary>
    /// <param name="sessionContext"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void EncodeData(NxdnSessionContext sessionContext)
    {
        if (sessionContext.TxAmbeData == null || sessionContext.TxAmbeData.Length < 28)
            throw new ArgumentException("TX AMBE data must be at least 28 bytes", nameof(sessionContext));

        // Test
        SetLichRfct(NXDN_LICH_RFCT_RDCH);
        SetLichFct(NXDN_LICH_USC_SACCH_SS);
        SetLichOption(NXDN_LICH_STEAL_NONE);
        SetLichDir(NXDN_LICH_DIRECTION_INBOUND);
        
        NxdnLich lich = NxdnLich.FromFields(
           rfct: NxdnRfct.Rdch,        // NXDN_LICH_RFCT_RDCH
           usc: NxdnUsc.SacchSs,      // NXDN_LICH_USC_SACCH_SS
           steal: NxdnSteal.None,     // NXDN_LICH_STEAL_NONE
           direction: NxdnDirection.Inbound // NXDN_LICH_DIRECTION_INBOUND
        );

        //_nxdnFrame[10] = GetLich();
        _nxdnFrame[10] = ComputeLichParity(lich.Raw); // TODO: check/verify this!

        SetSacchRan(0x01);

        SetLayer3MsgType(NXDN_MESSAGE_TYPE_VCALL);
        SetLayer3SrcId(sessionContext.NxdnId);
        SetLayer3DstId(sessionContext.GwId);
        SetLayer3Group(true);
        SetLayer3Blocks(0);

        byte[] msg = new byte[3];

        switch (sessionContext.TxFrameNumber % 4)
        {
            case 0:
                SetSacchStruct(3);
                Layer3Encode(msg, 18, 0);
                SetSacchData(msg);
                break;
            case 1:
                SetSacchStruct(2);
                Layer3Encode(msg, 18, 18);
                SetSacchData(msg);
                break;
            case 2:
                SetSacchStruct(1);
                Layer3Encode(msg, 18, 36);
                SetSacchData(msg);
                break;
            case 3:
                SetSacchStruct(0);
                Layer3Encode(msg, 18, 54);
                SetSacchData(msg);
                break;
        }

        WriteSacch(_nxdnFrame, 11);

        // Deinterleaving AMBE data ... 
        for (int i = 0; i < 4; i++)
        {
            DeinterleaveAmbe(sessionContext.TxAmbeData, i * 7);
        }

        Buffer.BlockCopy(sessionContext.TxAmbeData, 0, _nxdnFrame, 15, 7);
        for (int i = 0; i < 7; ++i)
        {
            _nxdnFrame[21 + i] |= (byte)(sessionContext.TxAmbeData[7 + i] >> 1);
            _nxdnFrame[22 + i] = (byte)((sessionContext.TxAmbeData[7 + i] & 1) << 7);
        }

        _nxdnFrame[28] |= (byte)(sessionContext.TxAmbeData[13] >> 2);

        Buffer.BlockCopy(sessionContext.TxAmbeData, 14, _nxdnFrame, 29, 7);

        for (int i = 0; i < 7; ++i)
        {
            _nxdnFrame[35 + i] |= (byte)(sessionContext.TxAmbeData[21 + i] >> 1);
            _nxdnFrame[36 + i] = (byte)((sessionContext.TxAmbeData[21 + i] & 1) << 7);
        }

        _nxdnFrame[41] |= (byte)(sessionContext.TxAmbeData[27] >> 2);
    }

    /// <summary>
    /// Encode a NXDN header frame.
    /// </summary>
    /// <param name="sessionCtx">details about the current session</param>
    private static void EncodeHeader(NxdnSessionContext sessionCtx)
    {
        // Test
        SetLichRfct(NXDN_LICH_RFCT_RDCH);
        SetLichFct(NXDN_LICH_USC_SACCH_NS);
        SetLichOption(NXDN_LICH_STEAL_FACCH);
        SetLichDir(NXDN_LICH_DIRECTION_INBOUND);

        // ---------------------------
        // Build LICH using FromFields()
        // ---------------------------
        NxdnLich lich = NxdnLich.FromFields(
            rfct: NxdnRfct.Rdch,        // NXDN_LICH_RFCT_RDCH
            usc: NxdnUsc.SacchNs,      // NXDN_LICH_USC_SACCH_NS
            steal: NxdnSteal.Facch,     // NXDN_LICH_STEAL_FACCH
            direction: NxdnDirection.Inbound
        );

        _nxdnFrame[10] = lich.Raw;

        // ---------------------------
        // SACCH
        // ---------------------------
        SetSacchRan(0x01);
        SetSacchStruct(0x00);
        SetSacchData(idle);

        // Writes 5 SACCH bytes at offset 11
        WriteSacch(_nxdnFrame, 11);

        // ---------------------------------------------------
        // Layer 3 - refers to the Network / Signalling Layer,
        // which carries system-level information such as:
        // - Source ID(SRCID)
        // - Destination ID(DSTID)
        // - Group or Individual call flag
        // - Message type(e.g., VCALL, TX_REL)
        // - Block counters
        // - Misc.call setup or teardown signalling
        // ---------------------------------------------------
        SetLayer3MsgType(sessionCtx.Eot ?  NXDN_MESSAGE_TYPE_TX_REL : NXDN_MESSAGE_TYPE_VCALL);

        SetLayer3SrcId(sessionCtx.NxdnId);
        SetLayer3DstId(sessionCtx.GwId);
        SetLayer3Group(true);
        SetLayer3Blocks(0);

        // Layer 3 appears twice
        Buffer.BlockCopy(_layer3, 0, _nxdnFrame, 15, 14);
        Buffer.BlockCopy(_layer3, 0, _nxdnFrame, 29, 14);
    }

    /// <summary>
    /// Deinterleaves the AMBE bits from a DVSI-encoded byte array segment. This method extracts 49 bits from the 7-byte segment starting at the given offset, 
    /// applies the DVSI deinterleaving mapping, and writes the resulting 7-byte AMBE payload back into the array at the same offset.
    /// </summary>
    /// <param name="buffer">data buffer containing the the 7-byte AMBE block to deinterleave</param>
    /// <param name="offset">defines where the AMBE block starts in the buffer</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static void DeinterleaveAmbe(byte[] buffer, int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (offset < 0 || offset + 7 > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must allow at least 7 bytes for an AMBE block.");

        byte[] dvsiData = new byte[49];
        byte[] ambeData = new byte[7];

        // Extract 49 individual bits into dvsiData[]
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                dvsiData[j + (8 * i)] = (byte)((buffer[offset + i] >> (7 - j)) & 0x01);
            }
        }

        // Last bit (49th bit)
        dvsiData[48] = (byte)((buffer[offset + 6] >> 7) & 0x01);

        // Reassemble AMBE bytes using interleave table
        for (int i = 0; i < 49; i++)
        {
            int j = dvsiInterleave[i];

            // Place bit into output byte
            ambeData[i / 8] |= (byte)(dvsiData[j] << (7 - (i % 8)));
        }

        // Copy back to original buffer at the given offset
        Buffer.BlockCopy(ambeData, 0, buffer, offset, 7);
    }

    /// <summary>
    /// Interleaves a 7-byte AMBE voice payload into a DVSI-encoded format. This method takes the 7-byte AMBE array, extracts 49 bits from the first 7 bytes,
    /// and applies the DVSI interleaving pattern defined in {@code dvsiInterleave}. The resulting 7-byte interleaved DVSI payload is written back into the 
    /// original array. Bit extraction is performed MSB first for each byte. The last bit (bit 48) is taken from the most significant bit of the 7th byte.
    /// </summary>
    /// <param name="ambe">a 7-byte AMBE block</param>
    /// <exception cref="ArgumentException"></exception>
    private static void Interleave(byte[] ambe)
    {
        if (ambe == null || ambe.Length < 7)
            throw new ArgumentException("AMBE array must contain at least 7 bytes.", nameof(ambe));

        byte[] ambeData = new byte[49];
        byte[] dvsiData = new byte[7];  // auto-initialized to zero

        // Extract bits from the first 7 bytes
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                ambeData[j + 8 * i] = (byte)(1 & (ambe[i] >> (7 - j)));
            }
        }

        ambeData[48] = (byte)(1 & (ambe[6] >> 7));

        for (int i = 0; i < dvsiInterleave.Length; i++)
        {
            int j = dvsiInterleave[i];
            dvsiData[j / 8] += (byte)(ambeData[i] << (7 - (j % 8)));
        }

        // Copy back
        Array.Copy(dvsiData, 0, ambe, 0, 7);
    }

    /// <summary>
    /// Computes and appends the 6-bit NXDN CRC to a bitstream.
    /// </summary>
    /// <param name="buffer">contains the bitstream to update. The CRC is written directly into this buffer starting at bit index <paramref name="bitLength"</param>
    /// <param name="offset">defines where the data to calculate start in the buffer.</param>
    /// <param name="bitLength">The number of payload bits over which the CRC is computed. 
    /// Bits [0 .. len-1] are used for CRC calculation, and bits [len .. len+5] are overwritten
    /// with the resulting 6 CRC bits.
    /// </param>
    static void EncodeCrc6(byte[] buffer, int offset, int bitLength)
    {
        byte crc = 0x3F;

        for (int i = 0; i < bitLength; i++)
        {
            bool bit1 = ReadBit(buffer, offset * 8 + i);
            bool bit2 = (crc & 0x20) != 0;
            crc <<= 1;

            if (bit1 ^ bit2)
                crc ^= 0x27;
        }

        crc &= 0x3F;

        int bitIndex = bitLength;
        for (int i = 2; i < 8; i++, bitIndex++)
        {
            bool b = ((crc >> i) & 1) != 0;
            WriteBit1(buffer, offset * 8 + bitIndex, b);
        }
    }


    static void Layer3Encode(byte[] data, int len, int offset)
    {
        for (int i = 0; i < len; i++, offset++)
        {
            bool bit = ReadBit(_layer3, offset);
            WriteBit1(data, i, bit);
        }
    }

    static void SetLayer3MsgType(byte arg)
    {
        _layer3[0] &= 0xC0;
        _layer3[0] |= (byte)(arg & 0x3F);
    }

    static void SetLayer3SrcId(int srcId)
    {
        _layer3[3] = (byte)((srcId >> 8) & 0xFF);
        _layer3[4] = (byte)((srcId >> 0) & 0xFF);
    }

    static void SetLayer3DstId(int dstId)
    {
        _layer3[5] = (byte)((dstId >> 8) & 0xFF);
        _layer3[6] = (byte)((dstId >> 0) & 0xFF);
    }

    static void SetLayer3Group(bool arg)
    {
        _layer3[2] = (byte)(arg ? (_layer3[2] | 0x20)   // set bit 5
                   : (_layer3[2] & ~0x20));             // clear bit 5
    }

    static void SetLayer3Blocks(byte arg)
    {
        _layer3[8] &= 0xF0;
        _layer3[8] |= (byte)(arg & 0x0F);
    }

    static byte ComputeLichParity(byte lich)
    {
        // Extract upper 4 bits
        byte high = (byte)((lich >> 4) & 0x0F);

        // Count 1-bits
        int ones = 0;
        for (int i = 0; i < 4; i++)
            ones += (high >> i) & 1;

        // Odd parity: if ones is even → parity bit = 1
        bool parity = (ones % 2 == 0);

        // Apply parity bit to LSB
        lich = parity ? (byte)(lich | 0x01) : (byte)(lich & 0xFE);

        return lich;
    }


    static byte GetLichFct(byte lich)
    {
        return (byte)((lich >> 4) & 0x03);
    }

    static void SetLichFct(byte fct)
    {
        _lich &= 0xCF;
        _lich |= (byte)((fct << 4) & 0x30);
    }


    static void SetLichRfct(byte rfct)
    {
        _lich &= 0x3F;
        _lich |= (byte)((rfct << 6) & 0xC0);
    }

    static void SetLichOption(byte opt)
    {
        _lich &= 0xF3;
        _lich |= (byte)((opt << 2) & 0x0C);
    }

    static void SetLichDir(byte dir)
    {
        _lich &= 0xFD;
        _lich |= (byte)((dir << 1) & 0x02);
    }

    // TODO: method changes _lich?!
    static byte GetLich()
    {
        bool parity = (_lich & 0xF0) switch
        {
            0x80 or 0xB0 => true,
            _ => false,
        };
        if (parity)
            _lich |= 0x01;
        else
            _lich &= 0xFE;

        return _lich;
    }

    // 
    // SACCH stuff (Slow Associated Control Channel)
    //
    //  SACCH carries low-rate control and signaling information.
    //  It rides alongside voice frames — kind of like a little side-car of control bits.
    //  - It usually transports things like:
    //  - RAN (Radio Access Number)
    //  - structure flags
    //  - link control bits
    //  - status signaling
    //  - sometimes idle patterns or small protocol messages

    /// <summary>
    /// Write the SACCH data to the NXDND frame.
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="offset"></param>
    static void WriteSacch(byte[] frame, int offset)
    {
        Buffer.BlockCopy(_sacch, 0, frame, offset, 4);
        EncodeCrc6(frame, offset, 26);
    }

    static void SetSacchData(byte[] data)
    {
        int offset = 8;
        for (int i = 0; i < 18; i++, offset++)
        {
            bool b = ReadBit(data, i);
            WriteBit1(_sacch, offset, b);
        }
    }

    /// <summary>
    /// Set the radio access number.
    /// </summary>
    /// <param name="ran">the radio access number to set</param>
    static void SetSacchRan(byte ran)
    {
        _sacch[0] &= 0xC0;
        _sacch[0] |= ran;
    }

    /// <summary>
    /// Sets the SACCH Structure field inside the SACCH header.
    /// 
    /// The SACCH “Structure” value indicates whether this SACCH message is:
    ///   - 0: A single, standalone SACCH block
    ///   - 1: The first block of a multi-block SACCH sequence
    ///   - 2: A middle block of a multi-block sequence
    ///   - 3: The final block of a multi-block sequence
    ///
    /// This field occupies the top two bits (bits 7–6) of the first SACCH byte.
    /// The method clears these bits and writes the new structure value.
    /// </summary>
    /// <param name="s">The 2-bit SACCH structure indicator (0–3).</param>
    static void SetSacchStruct(byte s)
    {
        _sacch[0] &= 0x3F;
        _sacch[0] |= (byte)((s << 6) & 0xC0); ;
    }

}
