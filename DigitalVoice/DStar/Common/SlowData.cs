using NLog;
using System.Text;
using System.Text.RegularExpressions;

namespace DigitalVoice.DStar.Common;

/// <summary>
/// Class to decode DStar Slow Data (user message and GPS data).
/// </summary>
public static class SlowData
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // XOR masks used in the protocol to decode slow data
    private const byte Mask1 = 0x70;
    private const byte Mask2 = 0x4F;
    private const byte Mask3 = 0x93;


    /// <summary>
    /// Collects and validates incoming GPS data bytes, assembling sentences into a buffer.  
    /// Returns <c>true</c> when a complete GPS sentence (ending with CR/LF) has been received.  
    /// </summary>
    /// <param name="b">The incoming GPS data byte.</param>
    /// <param name="clientState">The client state holding GPS buffers and counters.</param>
    /// <returns><c>true</c> if a full GPS sentence is completed; otherwise <c>false</c>.</returns>
    static bool CollectGpsData(byte b, DStarSessionContext clientState)
    {
        bool result = false;
        switch (b)
        {
            case 0x0A:
            case 0x0D:
                // TODO: check consistency
                string gpsRaw = clientState.RxGpsDataBuffer.ToString();
                logger.Debug($"{b:X2} '{gpsRaw}'");

                string gpsParsed = ParseGpsData(gpsRaw);
                if (!string.IsNullOrEmpty(gpsParsed))
                    clientState.RxGpsData = gpsParsed;
               
                clientState.RxGpsDataBuffer.Clear();
                clientState.SdGpsCnt = 0;
                result = true;
                break;
            case (byte)'$':
                clientState.RxGpsDataBuffer.Append((char)b);
                clientState.SdGpsCnt = clientState.SdGpsCnt > 0 ? clientState.SdGpsCnt - 1 : 0;
                // TODO: check consistencey
                break;
            default:
                if (b >= 0x20 && b <= 0x7E) // ASCII char? //  if (Ascii.IsValid(b))
                {
                    if (clientState.RxGpsDataBuffer.Length > 0 && clientState.RxGpsDataBuffer[0] == '$')
                    {
                        clientState.RxGpsDataBuffer.Append((char)b);
                    }
                    //else
                    //{
                    //    logger.Error($"GPS buffer not initialized, char 0x{b:X2} ignored.");
                    //}
                }
                else
                {
                    logger.Debug($"Invalid char: 0x{b:X2} (no ASCII) char ignored");
                }

                clientState.SdGpsCnt = clientState.SdGpsCnt > 0 ? clientState.SdGpsCnt - 1 : 0;
                break;
        }

        return result;
    }

    /// <summary>
    /// Decodes a D-STAR slow data frame from the given packet at the specified offset.  
    /// Handles SD sync detection, structured header parsing, GPS data extraction,  
    /// and user message reassembly across multiple frames. Updates the client state  
    /// with reconstructed GPS strings and user messages as they become available.  
    /// </summary>
    /// <param name="packet">The received D-STAR data packet.</param>
    /// <param name="offset">The byte offset where the SD frame begins.</param>
    /// <param name="clientState">The client state storing sync flags, counters, buffers, and decoded results.</param>
    public static void Decode(byte[] packet, int offset, DStarSessionContext clientState)
    {
        // Sequence number is always at offset
        byte seqNum = packet[offset];

        // 1st data bytes in each frame
        byte b0 = packet[offset + 10];
        byte b1 = packet[offset + 11];
        byte b2 = packet[offset + 12];

        // SD sync acquisition
        if ((seqNum == 0) && (b0 == 0x55) && (b1 == 0x2d) && (b2 == 0x16))
        {
            clientState.SdSync = true;
            clientState.SdSeq = 1;
        }

        // SD header
        byte c = (byte)(b0 ^ 0x70);
        if (clientState.SdSync && clientState.SdHdrCnt == 0 && (c & 0xF0) == 0x50)
        {
            // structured slow data header
            clientState.SdHdrCnt = (c & 0x0F); // number of chunks expected
            //logger.Debug($"SD header detected: chunks = {clientState.SdHdrCnt}");
        }
        else if (clientState.SdHdrCnt > 0)
        {
            c = 0;
            clientState.SdHdrCnt = 0;
        }

        if (clientState.SdSync && clientState.SdGpsCnt == 0 && (c & 0xF0) == 0x30)
        {
            clientState.SdGpsCnt = (c & 0x0F);
            c = (byte)(b1 ^ 0x4F);
            if (!CollectGpsData(c, clientState))
            {
                c = (byte)(b2 ^ 0x93);
                CollectGpsData(c, clientState);
            }
        }
        else if (clientState.SdGpsCnt > 0 && seqNum != 0)
        {
            if (!CollectGpsData(c, clientState))
            {
                c = (byte)(b1 ^ 0x4F);
                if (!CollectGpsData(c, clientState))
                {
                    c = (byte)(b2 ^ 0x93);
                    CollectGpsData(c, clientState);
                }
            }
            clientState.SdGpsCnt = 0;
        }

        switch (clientState.SdSeq)
        {
            case 1 when seqNum == 1 && b0 == 0x30:
                clientState.RxUserMsgBuffer[0] = (byte)(b1 ^ Mask2);
                clientState.RxUserMsgBuffer[1] = (byte)(b2 ^ Mask3);
                clientState.SdSeq++;
                break;

            case 2 when seqNum == 2:
                clientState.RxUserMsgBuffer[2] = (byte)(b0 ^ Mask1);
                clientState.RxUserMsgBuffer[3] = (byte)(b1 ^ Mask2);
                clientState.RxUserMsgBuffer[4] = (byte)(b2 ^ Mask3);
                clientState.SdSeq++;
                break;

            case 3 when seqNum == 3 && b0 == 0x31:
                clientState.RxUserMsgBuffer[5] = (byte)(b1 ^ Mask2);
                clientState.RxUserMsgBuffer[6] = (byte)(b2 ^ Mask3);
                clientState.SdSeq++;
                break;

            case 4 when seqNum == 4:
                clientState.RxUserMsgBuffer[7] = (byte)(b0 ^ Mask1);
                clientState.RxUserMsgBuffer[8] = (byte)(b1 ^ Mask2);
                clientState.RxUserMsgBuffer[9] = (byte)(b2 ^ Mask3);
                clientState.SdSeq++;
                break;

            case 5 when seqNum == 5 && b0 == 0x32:
                clientState.RxUserMsgBuffer[10] = (byte)(b1 ^ Mask2);
                clientState.RxUserMsgBuffer[11] = (byte)(b2 ^ Mask3);
                clientState.SdSeq++;
                break;

            case 6 when seqNum == 6:
                clientState.RxUserMsgBuffer[12] = (byte)(b0 ^ Mask1);
                clientState.RxUserMsgBuffer[13] = (byte)(b1 ^ Mask2);
                clientState.RxUserMsgBuffer[14] = (byte)(b2 ^ Mask3);
                clientState.SdSeq++;
                break;

            case 7 when seqNum == 7 && b0 == 0x33:
                clientState.RxUserMsgBuffer[15] = (byte)(b1 ^ Mask2);
                clientState.RxUserMsgBuffer[16] = (byte)(b2 ^ Mask3);
                clientState.SdSeq++;
                break;

            case 8 when seqNum == 8:
                clientState.RxUserMsgBuffer[17] = (byte)(b0 ^ Mask1);
                clientState.RxUserMsgBuffer[18] = (byte)(b1 ^ Mask2);
                clientState.RxUserMsgBuffer[19] = (byte)(b2 ^ Mask3);

                clientState.RxUsrMsg = Encoding.ASCII.GetString(clientState.RxUserMsgBuffer);
                clientState.SdSync = false;
                clientState.SdSeq = 0;

                logger.Debug($"clientState.RxUsrMsg = {clientState.RxUsrMsg}");
                break;
        }
    }

    /// <summary>
    /// Decodes a 32-byte D-STAR user header block from the packet at the given offset.  
    /// Splits the block into four 8-byte fields (RPT2, RPT1, URCALL, and source callsign),  
    /// trims padding spaces, and updates the corresponding properties in the client state.  
    /// </summary>
    /// <param name="packet">The received D-STAR data packet.</param>
    /// <param name="offset">The byte offset where the 32-byte header block begins.</param>
    /// <param name="clientState">The client state to update with decoded user header fields.</param>
    internal static void DecodeCallsigns(byte[] packet, int offset, DStarSessionContext clientState)
    {
        // Decode the whole 32-byte block once
        string headerBlock = Encoding.ASCII.GetString(packet, offset, 32);

        // Extract fields (8 bytes each), trimming trailing spaces
        clientState.RxRptr2 = headerBlock[..8].TrimEnd();
        clientState.RxRptr1 = headerBlock.Substring(8, 8).TrimEnd();
        clientState.RxUrCall = headerBlock.Substring(16, 8).TrimEnd();
        clientState.RxSrc = headerBlock.Substring(24, 8).TrimEnd();
    }


    /// <summary>
    /// Encodes a D-STAR slow data (SD) user message segment into the given packet.  
    /// The segment to encode is determined by the current transmit frame counter  
    /// (<see cref="DStarSessionContext.TxFrameCnt"/>), cycling through SD header and  
    /// 8 message fragments (20 ASCII bytes total).  
    /// </summary>
    /// <param name="packet">The target packet buffer to write into.</param>
    /// <param name="offset">The offset within the packet where encoding begins.</param>
    /// <param name="clientState">The client state containing the transmit frame counter and user message.</param>
    /// <remarks>
    /// - Frame 0 writes the SD sync header.  
    /// - Frames 1–8 encode the user message in 20 bytes across 8 chunks using XOR masks.  
    /// - A fallback pattern is written for other frame numbers.  
    /// - Frame counter (3 bytes) and marker byte (0x01) are appended at fixed positions.  
    /// </remarks>
    internal static void EncodeUsrMsg(byte[] packet, int offset, DStarSessionContext clientState)
    {
        packet[offset] = (byte)(clientState.TxFrameCnt % 21);

        switch (packet[offset])
        {
            case 0: // SD header
                packet[offset + 10] = 0x55;
                packet[offset + 11] = 0x2D;
                packet[offset + 12] = 0x16;
                break;
            case 1:
                packet[offset + 10] = (byte)(0x40 ^ Mask1);
                packet[offset + 11] = (byte)(clientState.TxUsrMsg[0] ^ Mask2);
                packet[offset + 12] = (byte)(clientState.TxUsrMsg[1] ^ Mask3);
                break;
            case 2:
                packet[offset + 10] = (byte)(clientState.TxUsrMsg[2] ^ Mask1);
                packet[offset + 11] = (byte)(clientState.TxUsrMsg[3] ^ Mask2);
                packet[offset + 12] = (byte)(clientState.TxUsrMsg[4] ^ Mask3);
                break;
            case 3:
                packet[offset + 10] = (byte)(0x41 ^ Mask1);
                packet[offset + 11] = (byte)(clientState.TxUsrMsg[5] ^ Mask2);
                packet[offset + 12] = (byte)(clientState.TxUsrMsg[6] ^ Mask3);
                break;
            case 4:
                packet[offset + 10] = (byte)(clientState.TxUsrMsg[7] ^ Mask1);
                packet[offset + 11] = (byte)(clientState.TxUsrMsg[8] ^ Mask2);
                packet[offset + 12] = (byte)(clientState.TxUsrMsg[9] ^ Mask3);
                break;
            case 5:
                packet[offset + 10] = (byte)(0x42 ^ Mask1);
                packet[offset + 11] = (byte)(clientState.TxUsrMsg[10] ^ Mask2);
                packet[offset + 12] = (byte)(clientState.TxUsrMsg[11] ^ Mask3);
                break;
            case 6:
                packet[offset + 10] = (byte)(clientState.TxUsrMsg[12] ^ Mask1);
                packet[offset + 11] = (byte)(clientState.TxUsrMsg[13] ^ Mask2);
                packet[offset + 12] = (byte)(clientState.TxUsrMsg[14] ^ Mask3);
                break;
            case 7:
                packet[offset + 10] = (byte)(0x43 ^ Mask1);
                packet[offset + 11] = (byte)(clientState.TxUsrMsg[15] ^ Mask2);
                packet[offset + 12] = (byte)(clientState.TxUsrMsg[16] ^ Mask3);
                break;
            case 8:
                packet[offset + 10] = (byte)(clientState.TxUsrMsg[17] ^ Mask1);
                packet[offset + 11] = (byte)(clientState.TxUsrMsg[18] ^ Mask2);
                packet[offset + 12] = (byte)(clientState.TxUsrMsg[19] ^ Mask3);
                break;
            default:
                packet[offset + 10] = 0x16;
                packet[offset + 11] = 0x29;
                packet[offset + 12] = 0xF5;
                break;
        }

        packet[offset + 13] = (byte)(clientState.TxFrameCnt & 0xFF);
        packet[offset + 14] = (byte)((clientState.TxFrameCnt >> 8) & 0xFF);
        packet[offset + 15] = (byte)((clientState.TxFrameCnt >> 16) & 0xFF);
        packet[offset + 16] = 0x01;
    }

    static readonly Regex reAltitude = new Regex(@"/A=(\d{1,6})");
    static readonly Regex reCoordinates_52 = new Regex(@"(\d{4}\.\d{2}[NS])\/(\d{5}\.\d{2}[EW])");

    static readonly Regex reCoordinates_710 = new Regex(@"(\d{4}\.\d{2}[NS])\\(\d{2}\.\d{3}[EW])");

    public static string ParseGpsData(string gpsData)
    {   
        StringBuilder buf = new();
        if (gpsData.StartsWith("$$CRC"))
        {

            if (gpsData.Contains(">API710"))
            {
                Match m = reCoordinates_710.Match(gpsData);
                if (m.Success)
                {
                    string latitude = m.Groups[1].Value;
                    string longitude = m.Groups[2].Value;
                    buf.Append(latitude[7]).Append(latitude[..2]).Append(' ').Append(latitude[2..7]).Append(' ');
                    buf.Append(longitude[6]).Append(longitude[..6]);
                }
            }
            else
            {
                Match m1 = reCoordinates_52.Match(gpsData);
                if (m1.Success)
                {
                    string latitude = m1.Groups[1].Value;
                    string longitude = m1.Groups[2].Value;
                    buf.Append(latitude[7]).Append(latitude[..2]).Append(' ').Append(latitude[2..7]).Append(' ');
                    buf.Append(longitude[8]).Append(longitude[..3]).Append(' ').Append(longitude[3..8]);
                }

                Match m2 = reAltitude.Match(gpsData);
                if (m2.Success)
                {
                    string altitude = m2.Groups[0].Value;
                    buf.Append(' ').Append(altitude);
                }
            }
        }
        else if (gpsData.StartsWith("$GPGGA"))
        {
            // NMEA
          
            // Parse checksum
            int parsedChecksum = -1;
            int n = gpsData.LastIndexOf('*');
            if (n >= 0)
            {
                n++;
                if (n < gpsData.Length)
                {
                    string hex = gpsData[n..];
                    if (hex.Length > 0)
                        parsedChecksum = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    return string.Empty; // unable to parse checksum
                }
            }

            // Calculate the checksum
            n--;
            byte calculatedChecksum = 0;
            for (int i = 1; i < n; i++)
            {
                char c = gpsData[i];
                if (c != ',')
                    calculatedChecksum ^= (byte)c;
            }

            if (parsedChecksum != calculatedChecksum)
                return string.Empty;

            string[] token = gpsData.Split(',');

            // Latitude
            if (token[3].Length == 1) 
                buf.Append(token[3]);
            else 
                return string.Empty;

            if (token[2].Length == 7)
            {
                buf.Append(token[2][0..2]).Append(' ');
                buf.Append(token[2][2..]).Append(' ');
            }
            else
                return string.Empty;

            // Longitude
            if (token[5].Length == 1)
                buf.Append(token[5]);
            else
                return string.Empty;

            if (token[4].Length == 8)
            {
                buf.Append(token[4][0..3]).Append(' ');
                buf.Append(token[4][3..]).Append(' ');
            }
            else
                return string.Empty;

            // Altitude
            buf.Append(token[9]);
            buf.Append(token[10].ToLower()); // unit
        }
        return buf.ToString();
    }
}