using NLog;
using System.Text;
using static FusionCodec.BitOperations;

namespace FusionCodec;

/// <summary>
/// Class that provides comprehensive methods to decode YSF packets. The methods are not thread safe, 
/// but since the processing of YSF packets is strictly serial, there is no need for this.
/// </summary>
public static class Decoder
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public const int YSF_CALLSIGN_LENGTH = 10;
    public const int YSF_FS_LEN = 5;
    public const int YSF_FICH_LEN = 25;

    internal readonly static byte[] FrameSync = [0xD4, 0x71, 0xC9, 0x63, 0x4D];

    internal static readonly int[] INTERLEAVE_TABLE_5_20 = [
           0, 40, 80, 120, 160,
           2, 42, 82, 122, 162,
           4, 44, 84, 124, 164,
           6, 46, 86, 126, 166,
           8, 48, 88, 128, 168,
          10, 50, 90, 130, 170,
          12, 52, 92, 132, 172,
          14, 54, 94, 134, 174,
          16, 56, 96, 136, 176,
          18, 58, 98, 138, 178,
          20, 60, 100, 140, 180,
          22, 62, 102, 142, 182,
          24, 64, 104, 144, 184,
          26, 66, 106, 146, 186,
          28, 68, 108, 148, 188,
          30, 70, 110, 150, 190,
          32, 72, 112, 152, 192,
          34, 74, 114, 154, 194,
          36, 76, 116, 156, 196,
          38, 78, 118, 158, 198];

    internal static readonly int[] INTERLEAVE_TABLE_9_20 = [
        0, 40, 80, 120, 160, 200, 240, 280, 320,
        2, 42, 82, 122, 162, 202, 242, 282, 322,
        4, 44, 84, 124, 164, 204, 244, 284, 324,
        6, 46, 86, 126, 166, 206, 246, 286, 326,
        8, 48, 88, 128, 168, 208, 248, 288, 328,
        10, 50, 90, 130, 170, 210, 250, 290, 330,
        12, 52, 92, 132, 172, 212, 252, 292, 332,
        14, 54, 94, 134, 174, 214, 254, 294, 334,
        16, 56, 96, 136, 176, 216, 256, 296, 336,
        18, 58, 98, 138, 178, 218, 258, 298, 338,
        20, 60, 100, 140, 180, 220, 260, 300, 340,
        22, 62, 102, 142, 182, 222, 262, 302, 342,
        24, 64, 104, 144, 184, 224, 264, 304, 344,
        26, 66, 106, 146, 186, 226, 266, 306, 346,
        28, 68, 108, 148, 188, 228, 268, 308, 348,
        30, 70, 110, 150, 190, 230, 270, 310, 350,
        32, 72, 112, 152, 192, 232, 272, 312, 352,
        34, 74, 114, 154, 194, 234, 274, 314, 354,
        36, 76, 116, 156, 196, 236, 276, 316, 356,
        38, 78, 118, 158, 198, 238, 278, 318, 358
    ];

    internal static readonly int[] INTERLEAVE_TABLE_26_4 = [
        0, 4,  8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100,
        1, 5,  9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49, 53, 57, 61, 65, 69, 73, 77, 81, 85, 89, 93, 97, 101,
        2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50, 54, 58, 62, 66, 70, 74, 78, 82, 86, 90, 94, 98, 102,
        3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51, 55, 59, 63, 67, 71, 75, 79, 83, 87, 91, 95, 99, 103];

    internal static readonly int[] DVSI_INTERLEAVE = [
        0, 3, 6,  9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 41, 43, 45, 47,
        1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34, 37, 40, 42, 44, 46, 48,
        2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35, 38];

    internal static readonly int[] IMBE_INTERLEAVE = [
        0,  7, 12, 19, 24, 31, 36, 43, 48, 55, 60, 67, 72, 79, 84, 91,  96, 103, 108, 115, 120, 127, 132, 139,
        1,  6, 13, 18, 25, 30, 37, 42, 49, 54, 61, 66, 73, 78, 85, 90,  97, 102, 109, 114, 121, 126, 133, 138,
        2,  9, 14, 21, 26, 33, 38, 45, 50, 57, 62, 69, 74, 81, 86, 93,  98, 105, 110, 117, 122, 129, 134, 141,
        3,  8, 15, 20, 27, 32, 39, 44, 51, 56, 63, 68, 75, 80, 87, 92,  99, 104, 111, 116, 123, 128, 135, 140,
        4, 11, 16, 23, 28, 35, 40, 47, 52, 59, 64, 71, 76, 83, 88, 95, 100, 107, 112, 119, 124, 131, 136, 143,
        5, 10, 17, 22, 29, 34, 41, 46, 53, 58, 65, 70, 77, 82, 89, 94, 101, 106, 113, 118, 125, 130, 137, 142];

    internal static readonly int[] AMBE_OUTPUT_BIT_INTERLEAVE_TABLE =
    [
        0, 18, 36, 1, 19, 37, 2, 20, 38, 3,
        21, 39, 4, 22, 40, 5, 23, 41, 6, 24,
        42, 7, 25, 43, 8, 26, 44, 9, 27, 45,
        10, 28, 46, 11, 29, 47, 12, 30, 48, 13,
        31, 14, 32, 15, 33, 16, 34, 17, 35
    ];

    internal static readonly byte[] FRAME_SYNC = [0xD4, 0x71, 0xC9, 0x63, 0x4D];

    internal static readonly byte[] WHITENING_DATA = [
        0x93, 0xD7, 0x51, 0x21, 0x9C, 0x2F, 0x6C, 0xD0, 0xEF, 0x0F,
        0xF8, 0x3D, 0xF1, 0x73, 0x20, 0x94, 0xED, 0x1E, 0x7C, 0xD8
    ];

    static readonly Convolution _ysfConvolution = new();

    /// <summary>
    /// Decode YSF/DCS packets.
    /// </summary>
    /// <param name="isYsf">Controls what type of packet to decode: true -> Ysf, false -> FCS.</param>
    /// <param name="packet">The data to decode (HC ,CC, or TC).</param>
    /// <exception cref="ArgumentException"></exception>
    public static DecodeResult Decode(bool isYsf, byte[] packet)
    {
        int expected = isYsf ? 155 : 130;
        if (packet.Length != expected)
            throw new ArgumentException($"{(isYsf ? "YSF" : "FCS")} packet must be {expected} bytes, got {packet.Length}");

        DecodeResult result = new();

        if (isYsf)
        {
            // Extract callsigns (10 bytes each, ASCII, strip padding)
            string calls = Encoding.ASCII.GetString(packet, 4, 30);
            result.Gw = calls[..10].Trim();
            result.Src = calls.Substring(10, 10).Trim();
            result.Dst = calls.Substring(20, 10).Trim();

            if (result.Dst == "ALL")
                result.Dst = "**********";

            // Status byte
            result.IsLastFrame = (packet[34] & 0x01) != 0;
            result.FrameCount = packet[34] >> 1;
        }
        else
        {
            // Status byte        
            result.IsLastFrame = (packet[120] & 0x01) != 0;
            result.FrameCount = packet[120] >> 1;
            result.Gw = Encoding.ASCII.GetString(packet, 121, 8).Trim();
        }

        int offset = isYsf ? 35 : 0; // point at frame sync
        if (!packet.AsSpan(offset, FrameSync.Length).SequenceEqual(FrameSync))
        {
            logger.Error($"Unable to assert the FS: {Convert.ToHexString(packet)} - packet ignored");
            return result; // TODO: really exit/exception
        }

        offset += 5; // point at FICH
        if (!Fich.Decode(packet, offset: offset))
        {
            logger.Error($"Unable to decode FICH, packet: {Convert.ToHexString(packet)} - packet ignored");
            return result; // game over
        }

        result.Cm = (CallMode)Fich.GetCM();  // call mode (00:Group/CQ mode, 01: Radio ID mode, 11:Individual mode)
        result.Fi = (FrameInformation)Fich.GetFI();    // frame information (channel type of the frame). 
        //                       00: Header Channel (HC), 
        //                       01: Communication Channel (CC), 
        //                       10: Terminator Channel (TC), 
        //                       11: Test Channel)
        result.Fn = Fich.GetFN(); // frame number (Shows the frame number when dividing and sending data.)
        result.Ft = Fich.GetFT(); // frame total (Shows the frame total when dividing and sending data.)
        result.Dt = (DataType)Fich.GetDT(); // data type (Shows the frame data type: 
        //                       00: V/D mode type 1 (simultaneous voice data communication mode 1), 
        //                       01: Data FR mode(high speed data transmission mode), 
        //                       10: V/D mode type 2 (simultaneous voice/data communication mode 2), 
        //                       11: Voice FR mode (high quality voice full rate mode))

        switch (result.Fi)
        {
            case FrameInformation.HC:
                (bool valid, string? dst, string? src) = DecodeHeader(true, packet, offset: isYsf ? 65 : 30);
                if (valid)
                {
                    result.Dst = dst ?? "";
                    result.Gw = result.Dst;
                    result.Src = src ?? "";
                }

                // Possibly update internal state (ignored result)
                //Decoder.DecodeHeader(packet, false, clientState);
                //logger.Debug($"HC: ");

                break;
            case FrameInformation.CC:
                // Streaming voice/data
                if (result.Dt == DataType.VOICE_FR)
                {
                    var imbeBlocks = DecodeVW(packet[65..]); // 65 = 35+5+25
                    imbeBlocks.ForEach(b => result.AmbeData.Add(b));
                    // TODO: kann so nicht bleiben
                }
                else if (result.Dt != DataType.DATA_FR)
                {
                    if (result.Fn is 0 or 1)
                    {
                        offset += 25; // point after FICH where the payload starts
                        byte[]? vdModeData = result.Dt switch
                        {
                            DataType.VD_MODE_1 => DecodeVD1(packet, offset),
                            DataType.VD_MODE_2 => DecodeVD2(packet, offset),
                            _ => null
                        };

                        string? decoded = vdModeData != null ? Encoding.ASCII.GetString(vdModeData).Trim() : null;  // TrimEnd('\0')
                        logger.Debug($"FN={result.Fn}, decoded='{decoded}'");
                        if (result.Fn == 0)
                            result.Dst = decoded;
                        else
                            result.Src = decoded;
                    }
                    else if (result.Fn is 6 or 7)
                    {
                        // TODO: get radio name/id
                    }

                    DecodeDn(packet, isYsf ? 65 : 30, result.AmbeData);
                }
                break;
            case FrameInformation.TC:
                break;
        }

        logger.Debug($"{result}");
        return result;
    }


    /// <summary>
    /// Decodes a YSF or FCS header from the provided packet and extracts source and destination callsigns.
    /// This method reconstructs and decodes the 45-byte DCH (Digital Communication Header) from the interleaved
    /// and convolutionally encoded data contained within the packet. It performs deinterleaving, convolutional
    /// decoding, CRC validation, and whitening reversal before extracting ASCII-encoded callsigns.
    /// </summary>
    /// <param name="firstHeader">Controls whether to decode the first or second header (DCH-1(0..4) vs DCH-2(0..4)).</param>
    /// <param name="packet">The byte array containing the encoded header data to decode.</param>
    /// <param name="offset">Defines where the payload data start within the packet.</param>
    /// <returns>A tuple containing:
    /// <list type="bullet">
    /// <item><description><c>bool</c> – True if CRC validation succeeds, otherwise false.</description></item>
    /// <item><description><c>string?</c> – The destination callsign if valid, otherwise null.</description></item>
    /// <item><description><c>string?</c> – The source callsign if valid, otherwise null.</description></item>
    /// </list>
    /// </returns>
    public static (bool, string?, string?) DecodeHeader(bool firstHeader, byte[] packet, int offset=65)
    {
        byte[] dch = new byte[45];
        int data_i = offset + (firstHeader ? 0 : 9); // DCH-1(0) or DCH-2(0)
        int dch_i = 0;

        for (int i = 0; i < 5; i++)
        {
            Buffer.BlockCopy(packet, data_i, dch, dch_i, 9);
            data_i += 18;
            dch_i += 9;
        }

        _ysfConvolution.Start();
        for (int i = 0; i < 180; i++)
        {
            int n = INTERLEAVE_TABLE_9_20[i];
            int s0 = ReadBit1(dch, n);
            int s1 = ReadBit1(dch, n + 1);
            _ysfConvolution.Decode(s0, s1);
        }
        byte[] output = new byte[23]; // Room for 22 bytes + a bit extra
        _ysfConvolution.Chainback(output, 176);

        string? dst = null;
        string? src = null;
        bool valid = Crc.CheckCcitt162(output, 22);
        if (valid)
        {
            for (int i = 0; i < 20; i++)
                output[i] ^= WHITENING_DATA[i];

            dst = Encoding.ASCII.GetString(output, 0, YSF_CALLSIGN_LENGTH).Trim();
            src = Encoding.ASCII.GetString(output, YSF_CALLSIGN_LENGTH, YSF_CALLSIGN_LENGTH).Trim();
        }
        logger.Debug($"CRC valid={valid}, dst={dst}, src={src}");
        return (valid, dst, src);
    }

    /// <summary>
    /// Extracts and decodes five AMBE voice blocks from a YSF DN (Digital Narrow) packet.
    /// This method performs the following steps for each of the 5 embedded 18-byte voice blocks:
    /// <list type="number">
    /// <item>Deinterleaves the encoded voice bits using a 26x4 interleave table.</item>
    /// <item>Applies whitening removal via XOR with a known pattern.</item>
    /// <item>Corrects errors using majority voting on 27 triplets of bits.</item>
    /// <item>Appends the remaining 22 bits untouched.</item>
    /// <item>Applies a final interleaving using a 49-bit permutation table.</item>
    /// </list>
    /// 
    /// <para>
    /// The result is a list of 5 AMBE voice frames, each 7 bytes (49 bits) in size.
    /// </para>
    ///
    /// </summary>
    /// <param name="packet">The full input YSF/FCS packet.</param>
    /// <param name="offset">Defines where the payload data start within the packet.</param>
    /// <param name="ambeData">The decoded AMBE data are store here.</param>
    public static void DecodeDn(byte[] packet, int offset, List<byte[]> ambeData)
    {
        int offset_2 = 5 * 8; // skip DCH(0) header = 5 bytes

        for (int j = 0; j < 5; j++, offset_2 += 144) // each 18-byte voice block = 144 bits
        {
            // Step 1: Deinterleave
            byte[] vch = new byte[13];
            for (int i = 0; i < 104; i++)
            {
                int n = INTERLEAVE_TABLE_26_4[i];
                bool s = ReadBit(packet, offset_2 + n + offset * 8);
                WriteBit1(vch, i, s);
            }

            // Step 2: Whitening removal
            for (int i = 0; i < 13; i++)
                vch[i] ^= WHITENING_DATA[i];

            // Step 3: Error correction using majority vote
            byte[] corrected = new byte[7];
            for (int i = 0; i < 27; i++) // 27 groups of 3 bits
            {
                int idx = i * 3;
                int count = 0;
                for (int k = 0; k < 3; k++)
                {
                    if (ReadBit(vch, idx + k))
                        count++;
                }

                bool bit = count >= 2; // majority vote
                WriteBit1(corrected, i, bit);
            }

            // Step 4: Copy remaining 22 bits unchanged
            for (int i = 0; i < 22; i++)
            {
                bool b = ReadBit(vch, 81 + i);
                WriteBit1(corrected, 27 + i, b);
            }

            // Step 5: Final interleaving using INTER_TABBIE_VCH49
            byte[] v_tmp = new byte[7];
            for (int i = 0; i < 49; i++)
            {
                bool b = ReadBit(corrected, AMBE_OUTPUT_BIT_INTERLEAVE_TABLE[i]);
                WriteBit1(v_tmp, i, b);
            }

            ambeData.Add(v_tmp);
        }
    }

    /// <summary>
    /// Extracts and decodes five AMBE voice blocks from a YSF DN (Digital Narrow) packet. It does the same as method DecodeDn but with less performance.
    /// <para>
    /// The major work is delegated to the method AmbeExtractor1.ExtractAmbe. 
    /// </para>
    /// </summary>
    /// <param name="packet">The input YSF packet (typically 100+ bytes).</param>
    /// <param name="offset">The byte offset within the packet where voice data begins (default is 65).</param>
    /// <returns>A list of five 7-byte AMBE voice blocks.</returns>
    public static List<byte[]> DecodeDn2(byte[] packet, int offset = 65)
    {
        List<byte[]> result = [];

        for (int i = 0; i < 5; i++) // loop thru voice blocks
        {
            byte[] b = new byte[13];
            Buffer.BlockCopy(packet, offset + 5 + i * 18, b, 0, b.Length); // take the last 13 bytes of the 18-bytes voice block
            byte[] ambe = AmbeExtractor1.ExtractAmbe(b);
            result.Add(ambe);
        }

        return result;
    }

    /// <summary>
    /// Decodes a VD=1 (Voice Data type 1) data segment from a YSF (Yaesu System Fusion) packet.
    /// This method performs the following steps:
    /// <list type="number">
    /// <item>Extracts 5 × 9-byte DCH segments (from 5 voice blocks).</item>
    /// <item>Applies bit deinterleaving using a 9×20 interleave table.</item>
    /// <item>Runs convolutional decoding using a Viterbi decoder.</item>
    /// <item>Performs CRC check on the resulting data (CCITT-16 over 22 bytes).</item>
    /// <item>If valid, removes the whitening from the first 20 bytes and returns them.</item>
    /// </list>
    ///
    /// </summary>
    /// <param name="packet">The full YSF packet (typically 100+ bytes).</param>
    /// <param name="offset">Defines where the VD starts within the packet.</param>
    /// <returns>
    /// A 20-byte decoded and dewhitened data block if CRC check passes; otherwise, <c>null</c>.
    /// </returns>
    public static byte[]? DecodeVD1(byte[] packet, int offset)
    {
        byte[] dch = new byte[45]; // 5*9 bytes

        int i_data = offset;
        int i_dch = 0;
        for (int i = 0; i < 5; i++)
        {
            Buffer.BlockCopy(packet, i_data, dch, i_dch, 9);
            i_data += 18;
            i_dch += 9;
        }
       
        _ysfConvolution.Start();

        for (int i = 0; i < 180; i++)
        {
            int n = INTERLEAVE_TABLE_9_20[i];
            byte s0 = (byte)ReadBit1(dch, n);
            byte s1 = (byte)ReadBit1(dch, n + 1);
            _ysfConvolution.Decode(s0, s1);
        }

        byte[] output = new byte[23];
        _ysfConvolution.Chainback(output, 176);

        if (Crc.CheckCcitt162(output, 22))
        {
            for (int i = 0; i < 20; i++)
            {
                output[i] ^= WHITENING_DATA[i];
            }
            return output[..20];
        }
        return null;
    }

    /// <summary>
    /// Decodes a VD=2 (Voice Data type 2) data segment from a YSF packet.
    ///
    /// This method performs the following steps:
    /// <list type="number">
    /// <item>Extracts 5 × 5-byte DCH segments (from 5 voice blocks).</item>
    /// <item>Applies bit deinterleaving using a 5×20 interleave table.</item>
    /// <item>Runs convolutional decoding using a Viterbi decoder.</item>
    /// <item>Performs CRC check on the resulting data (CCITT-16 over 12 bytes).</item>
    /// <item>If valid, removes the whitening from the first 10 bytes and returns the first 10 bytes (typically a callsign).</item>
    /// </list>
    ///
    /// </summary>
    /// <param name="packet">The full YSF packet (155/130 bytes).</param>
    /// <param name="offset">Defines where the Voice data start within the packet.</param>
    /// <returns>A 10-byte decoded and dewhitened data block if CRC check passes; otherwise, <c>null</c>.</returns>
    public static byte[]? DecodeVD2(byte[] packet, int offset)
    {
        byte[] dch = new byte[25]; // 5*5 bytes

        int iPacket = offset;
        int iDch = 0;
        for (int i = 0; i < 5; i++)
        {
            Buffer.BlockCopy(packet, iPacket, dch, iDch, 5);
            iPacket += 18;
            iDch += 5;
        }

        _ysfConvolution.Start();

        for (int i = 0; i < 100; i++)
        {
            int n = INTERLEAVE_TABLE_5_20[i];
            byte s0 = (byte)ReadBit1(dch, n);
            byte s1 = (byte)ReadBit1(dch, n + 1);
            _ysfConvolution.Decode(s0, s1);
        }

        byte[] output = new byte[13];
        _ysfConvolution.Chainback(output, 96);

        if (Crc.CheckCcitt162(output, 12))
        {
            for (int i = 0; i < 10; i++)
            {
                output[i] ^= WHITENING_DATA[i];
            }
            return output[..YSF_CALLSIGN_LENGTH];

            // TODO:
            //if ((fusion_extract.getFN() == 6 || fusion_extract.getFN() == 7) && (array4[5] == 85 || array4[5] == 84 || array4[5] == 83 || array4[5] == 3))
            //{
            //    fusion_extract.setradioID(array4[4]);
            //}

        }
        return null;
    }

    static string RadioName(byte id)
    {
        return id switch
        {
            9  => "DVMEGA-Cast",
            16 => "BlueDV",
            17 => "Peanut",
            21 => "DMR2YSF",
            22 => "IPSC2",
            27 => "FTM-100D",
            32 => "DR-2X",
            36 => "FT-1D",
            37 => "FTM-400D",
            38 => "DR-1X",
            39 => "FT-991",
            40 => "FT-2D",
            41 => "FTM-100D",
            43 => "FT-70D",
            44 => "FTM-3207D",
            46 => "FTM-7250D",
            48 => "FT-3D",
            49 => "FTM-300D",
            50 => "FTM-200D",
            51 => "FT-5D",
            52 => "FTM-500D",
            _ => id.ToString("X2") // return id as hex
        };
    }

    /// <summary>
    /// Decodes a YSF VW (Voice Wide) mode voice frame into a list of 5 IMBE frames.
    /// </summary>
    /// <param name="data">The full 155-byte YSF data packet containing voice data.</param>
    /// <returns>A list of five 11-byte IMBE frames extracted from the VW mode voice blocks.</returns>
    /// <remarks>
    /// This method performs the following steps for each of the 5 VCH blocks:
    /// <list type="number">
    ///   <item>Extracts an 18-byte VCH subframe from the packet.</item>
    ///   <item>Deinterleaves 144 bits using the IMBE_INTERLEAVE table.</item>
    ///   <item>Extracts the 12-bit C0 header and uses it to seed a pseudo-random number generator (PRNG).</item>
    ///   <item>Generates a 114-bit whitening vector using the PRNG and applies de-whitening to a portion of the bits.</item>
    ///   <item>Reconstructs the 88-bit IMBE frame by combining selected segments of the bitstream.</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the input data is not at least 155 bytes long.</exception>

    public static List<byte[]> DecodeVW(byte[] data)
    {
        List<byte[]> result = [];
        byte[] vch = new byte[18];

        int offset = 65; // point after FICH
        for (int j = 0; j < 5; j++, offset += 18)
        {
            Buffer.BlockCopy(data, offset, vch, 0, 18);

            bool[] bits = new bool[144];
            for (int i = 0; i < 144; i++)
            {
                int n = IMBE_INTERLEAVE[i];
                bits[i] = ReadBit(vch, n);
            }

            // Extract c0data (first 12 bits)
            uint c0data = 0;
            for (int i = 0; i < 12; i++)
            {
                c0data = c0data << 1 | (bits[i] ? 1u : 0u);
            }

            // Generate PRN whitening vector (PRN = Pseudo Random Number)
            bool[] prn = new bool[114];
            uint p = 16 * c0data;
            for (int i = 0; i < 114; i++)
            {
                p = (173 * p + 13849) % 65536;
                prn[i] = p >= 32768;
            }

            // De-whiten
            for (int i = 0; i < 114; i++)
            {
                bits[i + 23] ^= prn[i];
            }

            // Reconstruct IMBE frame from selected bits
            byte[] imbe = new byte[11];
            int bitOffset = 0;

            void WriteBits(int startIndex, int count)
            {
                for (int i = 0; i < count; i++, bitOffset++)
                    WriteBit1(imbe, bitOffset, bits[startIndex + i]);
            }

            WriteBits(0, 12);
            WriteBits(23, 12);
            WriteBits(46, 12);
            WriteBits(69, 12);
            WriteBits(92, 11);
            WriteBits(107, 11);
            WriteBits(122, 11);
            WriteBits(137, 7);

            result.Add(imbe);
        }
        return result;
    }

}