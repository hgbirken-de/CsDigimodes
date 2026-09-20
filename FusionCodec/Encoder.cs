using NLog;
using System.Text;
using static FusionCodec.BitOperations;

namespace FusionCodec;


/// <summary>
/// Class to encode YSF/FCS packets (HC, CC, and TC).
/// </summary>
public static class Encoder
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte[] ScrambleCode = [
        1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1,
        0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1,
        1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1,
        0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0,
        1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1,
        1, 1, 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 1,
        0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0,
        1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 1, 1, 1, 1, 0,
        0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0
        ]; // 180 bytes

    public const int CALLSIGN_LEN = 10;

    static readonly byte[] dcs1 = new byte[2 * CALLSIGN_LEN];
    static readonly byte[] dcs2 = new byte[2 * CALLSIGN_LEN];

    static readonly byte[] packetBuffer = new byte[155]; // buffer used to encode YSF/FCS packets
    
    static Encoder()
    {
        for (int i = 0; i < CALLSIGN_LEN; i++) 
            dcs1[i] = (byte)'*';
    }

    /// <summary>
    /// Builds and encodes a YSF or FCS header frame, including FICH configuration, callsign/repeater fields, and optional reflector information.
    /// </summary>
    /// <param name="isYsf">Controls if encoding is for YSF/FCS.</param>
    /// <param name="isEndOfTx">Controls if a Header- or Termintation-Channel is encoded.</param>
    /// <param name="frameNumber">The frame number to assign to the header.</param>
    /// <param name="callsign">The originating station callsign (max 10 chars).</param>
    /// <param name="reflName">The reflector name (max 8 chars). Required only for FCS.</param>
    /// <returns>A byte array containing the fully encoded header packet, ready for transmission.</returns>
    public static byte[] EncodeHeader(bool isYsf, bool isEndOfTx, int frameNumber, string callsign, string? reflName=null)
    {
        if (string.IsNullOrEmpty(callsign) || callsign.Length > CALLSIGN_LEN)
            throw new ArgumentException("Missing/Invalid argument", nameof(callsign));

        // Configure FICH
        Fich.SetFI((int)(isEndOfTx ? FrameInformation.TC : FrameInformation.HC));
        Fich.SetCS(2);
        Fich.SetCM(0);
        Fich.SetBN(0);
        Fich.SetBT(0);
        Fich.SetFN(0);
        Fich.SetFT(6);
        Fich.SetDev(false);
        Fich.SetMR(0);
        Fich.SetVoIP(false);
        Fich.SetDT((int)DataType.VD_MODE_2);
        Fich.SetSQL(false);
        Fich.SetSC(0);

        byte[] callsignBytes = Encoding.ASCII.GetBytes(callsign.PadRight(CALLSIGN_LEN));

        if (isYsf)
        {
            Encoding.ASCII.GetBytes("YSFD").CopyTo(packetBuffer, 0);
            callsignBytes.CopyTo(packetBuffer, 4);
            callsignBytes.CopyTo(packetBuffer, 4 + CALLSIGN_LEN);
            Encoding.ASCII.GetBytes("ALL".PadRight(CALLSIGN_LEN)).CopyTo(packetBuffer, 4 + CALLSIGN_LEN + CALLSIGN_LEN);
            packetBuffer[34] = (byte)(isEndOfTx ? (frameNumber & 0x7F) << 1 | 1 : 0x00); // status byte
            Decoder.FRAME_SYNC.CopyTo(packetBuffer, 35); // 5 bytes
            Fich.Encode(packetBuffer); // 25 bytes
        }
        else
        {
            if (string.IsNullOrEmpty(reflName) || reflName.Length > 8)
                throw new ArgumentException("Missing/Invalid argument", nameof(reflName));
            
            Decoder.FRAME_SYNC.CopyTo(packetBuffer, 0);
            Fich.Encode(packetBuffer, Decoder.FRAME_SYNC.Length); // 25 bytes
            packetBuffer[120] = 0x00;
            Encoding.ASCII.GetBytes(reflName.PadRight(8)).CopyTo(packetBuffer, 121);
            packetBuffer[129] = 0x00;
        }

        callsignBytes.CopyTo(dcs1, CALLSIGN_LEN);
        WriteDchData(true, dcs1, packetBuffer, isYsf ? 65 : 30);

        callsignBytes.CopyTo(dcs2, 0);
        callsignBytes.CopyTo(dcs2, CALLSIGN_LEN);
        WriteDchData(false, dcs2, packetBuffer, isYsf ? 65 : 30);

        return packetBuffer;
    }

    /// <summary>
    /// Encodes a single YSF/FCS frame (voice + data) with FICH, payload whitening, convolutional FEC, interleaving, and AMBE+VCH packing.
    /// </summary>
    /// <param name="clientState">Information about the client state.</param>
    /// <param name="callsign">The transmitting callsign in YSF mode (must not exceed <c>CALLSIGN_LENGTH</c> if YSF.</param>
    /// <param name="reflName">The reflector name in FCS mode (e.g. FCS00101) (must not exceed 8 characters).</param>
    /// <returns>A YSF/FCS CC packet</returns>
    /// <remarks>
    /// Processing steps:
    /// <list type="number">
    ///   <item>Configure the FICH (Frame Information Channel) with frame type, frame number (FN), and mode parameters.</item>
    ///   <item>
    ///     Build the frame header:
    ///     <list type="bullet">
    ///       <item>YSF mode → "YSFD", source callsign, repeater callsign, "ALL".</item>
    ///       <item>Repeater mode → Sync + FICH + callsign field.</item>
    ///     </list>
    ///   </item>
    ///   <item>
    ///     Prepare a 13-byte payload block:
    ///     <list type="bullet">
    ///       <item>Insert callsign, placeholders, or special patterns depending on FN.</item>
    ///       <item>Apply whitening mask to first 10 bytes.</item>
    ///       <item>Append 2-byte CCITT-16 CRC + pad.</item>
    ///     </list>
    ///   </item>
    ///   <item>
    ///     Apply convolutional encoding (rate 1/2, constraint length 5) → 25-byte FEC block.
    ///   </item>
    ///   <item>
    ///     Apply 5×20 interleaving pattern → interleaved FEC block.
    ///   </item>
    ///   <item>
    ///     For each of 5 AMBE blocks:
    ///     <list type="bullet">
    ///       <item>Insert 5-byte slice of interleaved FEC.</item>
    ///       <item>Extract 49 AMBE voice bits, apply DVSI bit interleaving.</item>
    ///       <item>Generate a 13-byte VCH block and insert into packet.</item>
    ///     </list>
    ///   </item>
    /// </list>
    /// </remarks>
    public static byte[] EncodeVD2(bool isYsf, int frameNumber, List<byte[]> ambeData, string callsign, string? reflName=null)
    {
        // Configure FICH
        int fn = (frameNumber - 1) % 7;
        Fich.SetFI((int)FrameInformation.CC);
        Fich.SetCS(2);
        Fich.SetCM(0);
        Fich.SetBN(0);
        Fich.SetBT(0);
        Fich.SetFN(fn);
        Fich.SetFT(6);
        Fich.SetDev(false);
        Fich.SetMR(0);
        Fich.SetVoIP(false);
        Fich.SetDT((int)DataType.VD_MODE_2);
        Fich.SetSQL(false);
        Fich.SetSC(0);

        if (isYsf)
        {
            if (string.IsNullOrEmpty(callsign) || callsign.Length > CALLSIGN_LEN)
                throw new ArgumentException($"Missing/invalid callsign: {callsign}", nameof(callsign));

            Encoding.ASCII.GetBytes("YSFD").CopyTo(packetBuffer, 0);
            Encoding.ASCII.GetBytes(callsign.PadRight(CALLSIGN_LEN)).CopyTo(packetBuffer, 4);
            Buffer.BlockCopy(packetBuffer, 4, packetBuffer, 4 + CALLSIGN_LEN, CALLSIGN_LEN);
            Encoding.ASCII.GetBytes("ALL".PadRight(CALLSIGN_LEN)).CopyTo(packetBuffer, 4 + CALLSIGN_LEN + CALLSIGN_LEN);
            packetBuffer[34] = (byte)((frameNumber & 0x7F) << 1);
            Decoder.FRAME_SYNC.CopyTo(packetBuffer, 35); // 5 bytes
            Fich.Encode(packetBuffer); // 25 bytes
        }
        else
        {
            if (string.IsNullOrEmpty(reflName) || reflName.Length > 8)
                throw new ArgumentException($"Missing/invalid reflector name: {reflName}", nameof(reflName));

            Decoder.FRAME_SYNC.CopyTo(packetBuffer, 0);
            Fich.Encode(packetBuffer, Decoder.FRAME_SYNC.Length); // 25 bytes
            packetBuffer[120] = 0x00;
            Encoding.ASCII.GetBytes(reflName.PadRight(8)).CopyTo(packetBuffer, 121);
            packetBuffer[129] = 0x00;
        }

        byte[] source = fn switch
        {
            0 => TenStars,
            1 or 2 or 3 => Encoding.ASCII.GetBytes(callsign.PadRight(10)),
            4 or 5 => TenSpaces,
            6 => ft70d1,
            7 => dt2_temp,
            _ => []
        };

        byte[] whitenedPayload = new byte[13];

        if (source.Length > 0)
            source.CopyTo(whitenedPayload, 0);

        // Step 1: whitening and CRC
        for (int i = 0; i < 10; i++)
            whitenedPayload[i] ^= Decoder.WHITENING_DATA[i];

        Crc.AddCcitt162(whitenedPayload, 12);
        whitenedPayload[12] = 0x00;

        // Step 2: convolution encode
        byte[] convolutionEncoded = new byte[25];
        Convolution.Encode(whitenedPayload, convolutionEncoded, 100);

        // Step 3: interleave
        byte[] fecInterleaved = new byte[25];
        int j = 0;
        for (int i = 0; i < 100; i++)
        {
            int n = Decoder.INTERLEAVE_TABLE_5_20[i];
            bool s0 = ReadBit(convolutionEncoded, j++);
            bool s1 = ReadBit(convolutionEncoded, j++);
            WriteBit1(fecInterleaved, n, s0);
            WriteBit1(fecInterleaved, n + 1, s1);
        }

        // Step 4: build AMBE+VCH
        int p1 = isYsf ? 65 : 30, p2 = 0;
        byte[] ambeBits = new byte[56];
        byte[] interleavedAmbeBits = new byte[56];

        for (int i = 0; i < 5; i++)
        {
            byte[] a = ambeData[i];
            Buffer.BlockCopy(fecInterleaved, p2, packetBuffer, p1, 5); // 5-byte chunk
            for (int k = 0; k < 7; k++) // unpack 7 bytes -> 56 bits
            {
                for (int k2 = 0; k2 < 8; k2++)
                {
                    //ambeBits[k2 + 8 * k] = (byte)((ambeBlocks[k + i*7] >> (7 - k2)) & 0x1);
                    //byte[] a = ambeData[i];
                    ambeBits[k2 + 8 * k] = (byte)(a[k] >> 7 - k2 & 0x1);
                }
            }

            // apply DVSI interleaving
            for (int k = 0; k < 49; k++)
                interleavedAmbeBits[k] = ambeBits[Decoder.DVSI_INTERLEAVE[k]];

            GenerateVoiceChannelVd2(interleavedAmbeBits).CopyTo(packetBuffer, p1 + 5); // copy 13-byte vch into stream

            p1 += 18; p2 += 5;
        }
        return packetBuffer;
    }

    private readonly static byte[] ft70d1 = [0x01, 0x22, 0x61, 0x5f, 0x2b, 0x03, 0x11, 0x00, 0x00, 0x00];
    private readonly static byte[] dt2_temp = [0x00, 0x00, 0x00, 0x00, 0x6C, 0x20, 0x1C, 0x20, 0x03, 0x08];
    private readonly static byte[] TenSpaces = Encoding.ASCII.GetBytes("".PadRight(10));
    private readonly static byte[] TenStars = Encoding.ASCII.GetBytes("**********");

    /// <summary>
    /// Encodes and inserts DCH-1/DCH-2 data (i.e. Callsign Data 1/2) into the argument buffer starting at a specified position.
    /// 
    /// Processing steps:
    /// <list type="number">
    ///   <item><description>Whitening: Apply the standard 20-byte YSF whitening sequence to the input payload.</description></item>
    ///   <item><description>CRC: Append a CCITT-16 (6-bit) error detection code.</description></item>
    ///   <item><description>Convolutional Encoding: Encode 180 input bits into 360 output bits using rate-1/2 convolution.</description></item>
    ///   <item><description>Interleaving: Reorder the encoded bits according to the 9×20 Fusion interleave table.</description></item>
    ///   <item><description>Framing: Copy the resulting 45 encoded bytes into the target packet buffer with proper stride, positioned after the FICH block.  
    ///     The insertion offset depends on <paramref name="isYsf"/> (YSF vs. FCS) and <paramref name="writeDch1"/> (adds 9-byte offset for odd/even slot placement).</description></item>
    /// </list>
    /// </summary>
    /// <param name="writeDch1">Constrols which kind of DCH data is written: true -> DCH-1, false -> DCH-2.</param>
    /// <param name="dcs">The Data Channel Segment data to write.</param>
    /// <param name="buffer">The buffer into which the DCH data is written.</param>
    /// <param name="offset">Defines where to start writing within the buffer.</param>
    public static void WriteDchData(bool writeDch1, byte[] dcs, byte[] buffer, int offset)
    {
        // 1. Whitening step
        byte[] whitenedPayload = new byte[25];
        for (int i = 0; i < dcs.Length; i++)
            whitenedPayload[i] = (byte)(dcs[i] ^ Decoder.WHITENING_DATA[i]);

        // 2. Add CRC
        Crc.AddCcitt162(whitenedPayload, 22);
        whitenedPayload[22] = 0x00;

        // 3. Convolution encode (180 bits)
        byte[] encodedBits = new byte[45];
        Convolution.Encode(whitenedPayload, encodedBits, 180);

        // 4. Interleaving step
        byte[] interleavedBits = new byte[45];
        int j = 0;
        for (int i = 0; i < 180; i++)
        {
            int n = Decoder.INTERLEAVE_TABLE_9_20[i];
            bool s0 = ReadBit(encodedBits, j++);
            bool s1 = ReadBit(encodedBits, j++);
            WriteBit1(interleavedBits, n, s0);
            WriteBit1(interleavedBits, n + 1, s1);
        }

        // 5. Framing: copy interleaved bytes into packet buffer with stride
        int p1 = offset + (writeDch1 ? 0 : 9); // point behind FICH block
        int p2 = 0;
        for (int i = 0; i < 5; i++)
        {
            Buffer.BlockCopy(interleavedBits, p2, buffer, p1, 9);
            p1 += 18;
            p2 += 9;
        }
    }


    /// <summary>
    /// Encodes a YSF VD Mode 2 data frame.
    /// </summary>
    /// <param name="dt">The input DT (data type) field containing 10 callsign/metadata bytes, which will be whitened and CRC-protected before convolutional encoding.</param>
    /// <param name="buffer">The output buffer into which the generated VD Mode 2 frame is written.</param>
    /// <param name="offset">The starting offset in <paramref name="buffer"/> where the encoded frame will be placed.</param>
    public static void WriteVDMode2Data(byte[] dt, byte[] buffer, int offset)
    {

        if (dt.Length != 10) 
            throw new ArgumentException($"Invalid length {dt.Length} (must be 10)", $"{nameof(dt)}");
        
        // Step 1: whitening and CRC
        byte[] dt_tmp = new byte[13];
        dt.CopyTo(dt_tmp, 0);

        for (int i = 0; i < 10; i++)
            dt_tmp[i] ^= Decoder.WHITENING_DATA[i];

        Crc.AddCcitt162(dt_tmp, 12);
        dt_tmp[12] = 0x00;

        // Step 2: convolution encode
        byte[] convolved = new byte[25];
        Convolution.Encode(dt_tmp, convolved, 100);

        // Step 3: interleave
        byte[] interleavedBytes = new byte[25];
        int j = 0;
        for (int i = 0; i < 100; i++)
        {
            int n = Decoder.INTERLEAVE_TABLE_5_20[i];
            bool s0 = ReadBit(convolved, j++);
            bool s1 = ReadBit(convolved, j++);
            WriteBit1(interleavedBytes, n, s0);
            WriteBit1(interleavedBytes, n + 1, s1);
        }

        // Step 4: write to buffer
        int p1 = offset;
        int p2 = 0;  
        for (int i = 0; i < 5; i++)
        {
            Buffer.BlockCopy(interleavedBytes, p2, buffer, p1, 5); // 5-byte chunk
            p1 += 18;
            p2 += 5;
        }
    }

    /// <summary>
    /// Generates a 13-byte Voice Channel (VCH) block for YSF VD Mode 2 frames from 49 interleaved AMBE voice bits.
    /// </summary>
    /// <param name="ambeBits">The AMBE voice bits (length >= 49, each element must be 0 or 1).</param>
    /// <returns>A 13-byte array representing the encoded VCH block, ready for transmission.</returns>
    /// <remarks>
    /// <para>
    /// Processing steps:
    /// <list type="number">
    ///   <item>Take 49 AMBE voice bits as input.</item>
    ///   <item>Repeat the first 27 bits three times (simple FEC) -> 81 bits.</item>
    ///   <item>Append the remaining 22 bits directly -> total 103 bits, then pad with one 0 -> 104 bits.</item>
    ///   <item>Scramble all 104 bits using a pseudo-random sequence.</item>
    ///   <item>Interleave the scrambled bits by transposing a 4×26 bit matrix.</item>
    ///   <item>Pack the resulting 104 bits into 13 bytes (8 bits per byte).</item>
    /// </list>
    /// </para>
    /// </remarks>
    static byte[] GenerateVoiceChannelVd2(byte[] ambeBits)
    {
        // Expanded bitstream before scrambling (FEC + pad)
        byte[] fecExpandedBits = new byte[104];

        // Interleaved (scrambled -> matrix-transposed) bits
        byte[] interleavedBits = new byte[104];

        // 1. Apply simple FEC: repeat first 27 bits three times -> 81 bits
        for (int i = 0; i < 27; i++)
        {
            int i3 = i * 3;
            fecExpandedBits[i3 + 0] = ambeBits[i];
            fecExpandedBits[i3 + 1] = ambeBits[i];
            fecExpandedBits[i3 + 2] = ambeBits[i];
        }

        // 2. Copy remaining 22 bits -> positions [81..102]
        Buffer.BlockCopy(ambeBits, 27, fecExpandedBits, 81, 22);

        // 3. Pad final bit -> index 103
        fecExpandedBits[103] = 0x00;

        // 4. Scramble in-place
        Scramble(fecExpandedBits, fecExpandedBits.Length);

        // 5. Interleave via 4×26 transposition
        const int rows = 4;
        const int cols = 26;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                interleavedBits[i + j * rows] = fecExpandedBits[j + i * cols];
            }
        }

        // 6. Pack 104 bits -> 13 bytes 
        byte[] vchBlock = new byte[13]; // Final packed 13-byte VCH block
        for (int i = 0; i < 13; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                vchBlock[i] |= (byte)(interleavedBits[i * 8 + j] << 7 - j);
            }
        }

        return vchBlock;
    }


    /// <summary>
    /// Helper method that (de)scrambles the argument buffer in place.
    /// </summary>
    /// <param name="buf">The buffer to (de)scramble.</param>
    /// <param name="len">The number of buffer bytes to (de)scramle.</param>
    static void Scramble(byte[] buf, int len)
    {
        for (int i = 0; i < len; i++)
            buf[i] ^= ScrambleCode[i];
    }

}