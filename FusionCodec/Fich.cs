using static FusionCodec.BitOperations;

namespace FusionCodec;

/// <summary>
/// Provides encoding, decoding, and manipulation methods for the YSF 
/// (Yaesu System Fusion) FICH (Frame Information Channel) field.
/// <para>
/// The FICH is a critical 6-byte header present in every YSF frame, 
/// carrying metadata such as frame type, data type, modulation, and
/// control flags. This class handles:
/// </para>
/// <list type="bullet">
/// <item><description>Encoding FICH fields with Golay (24,12) and convolutional error correction, including interleaving.</description></item>
/// <item><description>Decoding and error correction of received FICH fields.</description></item>
/// <item><description>Bit-level accessors (getters and setters) for all FICH sub-fields (FI, CM, BN, BT, FN, FT, DT, MR, Dev, VoIP, SQL, SQ).</description></item>
/// </list>
/// <para>
/// Internally, the class uses Golay (24,12) error correction, CCITT-16 cyclic redundancy checks, 
/// and a fixed convolutional encoder with interleaving, as defined in the YSF protocol specification.
/// </para>
/// <remarks>
/// This is a static helper class and is not thread-safe due to shared state in <c>m_fich</c>.
/// Callers must coordinate access when used in multi-threaded contexts.
/// </remarks>
/// </summary>
public static class Fich
{
    static byte[] m_fich = new byte[6];

    static readonly Convolution convolution = new();
    
    static readonly int[] INTERLEAVE_TABLE = [
        0, 40,  80, 120, 160,
        2, 42,  82, 122, 162,
        4, 44,  84, 124, 164,
        6, 46,  86, 126, 166,
        8, 48,  88, 128, 168,
        10, 50,  90, 130, 170,
        12, 52,  92, 132, 172,
        14, 54,  94, 134, 174,
        16, 56,  96, 136, 176,
        18, 58,  98, 138, 178,
        20, 60, 100, 140, 180,
        22, 62, 102, 142, 182,
        24, 64, 104, 144, 184,
        26, 66, 106, 146, 186,
        28, 68, 108, 148, 188,
        30, 70, 110, 150, 190,
        32, 72, 112, 152, 192,
        34, 74, 114, 154, 194,
        36, 76, 116, 156, 196,
        38, 78, 118, 158, 198
    ];


    /// <summary>
    /// Encodes the current FICH (Frame Information Channel) into a convolutionally
    /// encoded, interleaved bitstream ready for transmission in a YSF frame.
    /// </summary>
    /// <param name="byt">
    /// Destination buffer into which the encoded and interleaved FICH bits 
    /// will be written. Caller must ensure sufficient space is available.
    /// </param>
    /// <remarks>
    /// Steps performed:
    /// <list type="number">
    /// <item><description>Adds a CCITT-16 CRC over the 6-byte FICH field.</description></item>
    /// <item><description>Splits the 48-bit FICH into 4 × 12-bit codewords (three nibbles each).</description></item>
    /// <item><description>Encodes each 12-bit block with Golay (24,12) forward error correction.</description></item>
    /// <item><description>Convolutionally encodes the 96 Golay bits into 200 bits.</description></item>
    /// <item><description>Interleaves the convolutional output using the fixed YSF interleaving table and writes the result into the output buffer.</description></item>
    /// </list>
    /// </remarks>
    public static void Encode(byte[] byt, int offset=40)
    {
        Crc.AddCcitt162(m_fich, 6);

        int b0 = m_fich[0] << 4 & 0xFF0 | m_fich[1] >> 4 & 0x00F;
        int b1 = m_fich[1] << 8 & 0xF00 | m_fich[2] >> 0 & 0x0FF;
        int b2 = m_fich[3] << 4 & 0xFF0 | m_fich[4] >> 4 & 0x00F;
        int b3 = m_fich[4] << 8 & 0xF00 | m_fich[5] >> 0 & 0x0FF;

        int c0 = Golay24128.Encode24128(b0);
        int c1 = Golay24128.Encode24128(b1);
        int c2 = Golay24128.Encode24128(b2);
        int c3 = Golay24128.Encode24128(b3);

        byte[] conv =
        [
            (byte)(c0 >> 16 & 0xFF),
            (byte)(c0 >> 8 & 0xFF),
            (byte)(c0 >> 0 & 0xFF),
            (byte)(c1 >> 16 & 0xFF),
            (byte)(c1 >> 8 & 0xFF),
            (byte)(c1 >> 0 & 0xFF),
            (byte)(c2 >> 16 & 0xFF),
            (byte)(c2 >> 8 & 0xFF),
            (byte)(c2 >> 0 & 0xFF),
            (byte)(c3 >> 16 & 0xFF),
            (byte)(c3 >> 8 & 0xFF),
            (byte)(c3 >> 0 & 0xFF),
            0x00,
        ];

        convolution.Start();
        
        byte[] convolved = new byte[25];
        Convolution.Encode(conv, convolved, 100);

        int bitOffset = offset * 8; // convert byte offset to bit offset
        int j = 0;
        for (int i = 0; i < 100; i++)
        {
            int n = INTERLEAVE_TABLE[i];
            bool s0 = ReadBit1(convolved, j) != 0;
            j++;
            bool s1 = ReadBit1(convolved, j) != 0;
            j++;

            WriteBit1(byt, n + bitOffset, s0);

            n += 1;
            WriteBit1(byt, n + bitOffset, s1);
        }
    }

    /// <summary>
    /// Decodes a YSF Frame Information Channel (FICH) from a received packet.
    /// </summary>
    /// <param name="packet">
    /// The input packet containing the convolutionally encoded and interleaved FICH bits.
    /// </param>
    /// <param name="offset">
    /// The starting byte offset (default = 40) within the packet where the FICH region begins.
    /// </param>
    /// <returns>
    /// <c>true</c> if the decoded FICH passes the CCITT-16 CRC check; 
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Steps performed:
    /// <list type="number">
    /// <item><description>Reads 200 convolutionally encoded and interleaved bits from the packet starting at the given offset.</description></item>
    /// <item><description>Deinterleaves the bits using the fixed YSF interleaving table and passes them into the Viterbi decoder.</description></item>
    /// <item><description>Performs convolutional decoding and chainback to recover the original 96 Golay-protected bits.</description></item>
    /// <item><description>Splits the 96 bits into 4 × 24-bit Golay codewords and applies Golay (24,12) decoding to recover 4 × 12-bit words.</description></item>
    /// <item><description>Reassembles the 4 × 12-bit words into the original 6-byte FICH field.</description></item>
    /// <item><description>Verifies the CCITT-16 CRC over the decoded FICH.</description></item>
    /// </list>
    /// </remarks>
    public static bool Decode(byte[] packet, int offset=40)
    {
        convolution.Start();
        int bitOffset = offset * 8;
        for (int i = 0; i < 100; i++)
        {
            int n = INTERLEAVE_TABLE[i];
            int s0 = ReadBit1(packet, bitOffset + n) > 0 ? 1 : 0;

            n += 1;
            int s1 = ReadBit1(packet, bitOffset + n) > 0 ? 1 : 0;

            convolution.Decode(s0, s1);
        }

        byte[] output = new byte[13];
        convolution.Chainback(output, 96);

        int b0 = Golay24128.Decode24128([output[0], output[1], output[2]]);
        int b1 = Golay24128.Decode24128([output[3], output[4], output[5]]);
        int b2 = Golay24128.Decode24128([output[6], output[7], output[8]]);
        int b3 = Golay24128.Decode24128([output[9], output[10], output[11]]);

        m_fich = new byte[6];
        m_fich[0] = (byte)(b0 >> 4 & 0xFF);
        m_fich[1] = (byte)(b0 << 4 & 0xF0 | b1 >> 8 & 0x0F);
        m_fich[2] = (byte)(b1 >> 0 & 0xFF);
        m_fich[3] = (byte)(b2 >> 4 & 0xFF);
        m_fich[4] = (byte)(b2 << 4 & 0xF0 | b3 >> 8 & 0x0F);
        m_fich[5] = (byte)(b3 >> 0 & 0xFF);

        return Crc.CheckCcitt162(m_fich, 6);
    }

    /// <summary>
    /// Returns the frame information.
    /// </summary>
    /// <returns>0 = Header Channel (HC), 1 = Communication Channel (CC), 2 = Terminator Channel (TC), 3 = Test Channel)</returns>
    public static int GetFI()
    {
        return m_fich[0] >> 6 & 0x03;
    }

    /// <summary>
    /// Returns the call mode.
    /// </summary>
    /// <returns>0 = Group/CQ mode, 1 = Radio ID mode, 3 = Individual mode</returns>
    public static int GetCM()
    {
        return m_fich[0] >> 2 & 0x03;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static int GetBN()
    {
        return m_fich[0] & 0x03;
    }

    public static int GetBT()
    {
        return m_fich[1] >> 6 & 0x03;
    }

    /// <summary>
    /// Returns the frame number when dividing and sending data.
    /// </summary>
    /// <returns>The frame number</returns>
    public static int GetFN()
    {
        return m_fich[1] >> 3 & 0x07;
    }

    /// <summary>
    /// Returns the frame total (i.e. the frame total when dividing and sending data).
    /// </summary>
    /// <returns></returns>
    public static int GetFT()
    {
        return m_fich[1] & 0x07;
    }

    /// <summary>
    /// Returns the frame data type 0 = V/D mode type 1 (simultaneous voice data communication mode 1).
    /// </summary>
    /// <returns>1 = Data FR mode(high speed data transmission mode), 
    /// 2 = V/D mode type 2 (simultaneous voice/data communication mode 2), 
    ///  3 = Voice FR mode (high quality voice full rate mode))</returns>
    public static int GetDT()
    {
        return m_fich[2] & 0x03;
    }

    public static int GetMR()
    {
        return m_fich[2] >> 3 & 0x03;
    }

    public static bool GetDev()
    {
        return (m_fich[2] & 0x40) == 0x40;
    }

    public static bool GetVoIP()
    {
        return (m_fich[2] & 0x04) == 0x04;
    }

    public static bool GetSQL()
    {
        return (m_fich[3] & 0x80) == 0x80;
    }

    public static int GetSC()
    {
        return m_fich[3] & 0x7F;
    }

    public static void SetFI(int fi)
    {
        m_fich[0] &= 0x3F;
        m_fich[0] |= (byte)((fi & 0x03) << 6);
    }

    public static void SetCS(int cs)
    {
        m_fich[0] &= 0xCF;
        m_fich[0] |= (byte)((cs & 0x03) << 4);
    }

    public static void SetCM(int cm)
    {
        m_fich[0] &= 0xF3;
        m_fich[0] |= (byte)((cm & 0x03) << 2);
    }

    public static void SetBN(int bn)
    {
        m_fich[0] &= 0xFC;
        m_fich[0] |= (byte)(bn & 0x03);
    }

    public static void SetBT(int bt)
    {
        m_fich[1] &= 0x3F;
        m_fich[1] |= (byte)((bt & 0x03) << 6);
    }

    public static void SetFN(int fn)
    {
        m_fich[1] &= 0xC7;
        m_fich[1] |= (byte)((fn & 0x07) << 3);
    }

    public static void SetFT(int ft)
    {
        m_fich[1] &= 0xF8;
        m_fich[1] |= (byte)(ft & 0x07);
    }

    public static void SetDT(int dt)
    {
        m_fich[2] &= 0xFC;
        m_fich[2] |= (byte)(dt & 0x03);
    }

    public static void SetMR(int mr)
    {
        m_fich[2] &= 0xE7;
        m_fich[2] |= (byte)((mr & 0x03) << 3);
    }

    public static void SetDev(bool dev)
    {
        if (dev)
            m_fich[2] |= 0x40;
        else
            m_fich[2] &= 0xBF;
    }

    public static void SetVoIP(bool voip)
    {
        if (voip)
            m_fich[2] |= 0x04;
        else
            m_fich[2] &= 0xFB;
    }

    public static void SetSQL(bool sql)
    {
        if (sql)
            m_fich[3] |= 0x80;
        else
            m_fich[3] &= 0x7F;
    }

    public static void SetSC(int sq)
    {
        m_fich[3] &= 0x80;
        m_fich[3] |= (byte)(sq & 0x7F);
    }
}