using DigitalVoice.Common;
using DigitalVoice.DStar.Common;
using NLog;
using System.Text;

namespace DigitalVoice.DStar.Xrf;

internal static class XrfCodec
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte[] userMsg = new byte[20];

    internal static byte[] Create(DStarSessionContext state, byte[] ambeData)
    {
        return [];
    }

    /// <summary>
    /// Decodes a 32-byte D-STAR user header block from the packet.  
    /// Splits the block into four 8-byte fields (RPT2, RPT1, URCALL, and source callsign),  
    /// trims padding spaces, and updates the corresponding properties in the client state.  
    /// </summary>
    /// <param name="packet">The received D-STAR data packet.</param>
    /// <param name="clientState">The client state to update with decoded user header fields.</param>
    internal static void DecodeCallsigns(byte[] packet, DStarSessionContext clientState)
    {
        if (Encoding.ASCII.GetString(packet, 0, 4) == "DSVT")
        {
            // Decode the whole 32-byte block once
            string headerBlock = Encoding.ASCII.GetString(packet, 18, 32);

            // Extract fields (8 bytes each), trimming trailing spaces
            clientState.RxRptr2 = headerBlock[..8].TrimEnd();
            clientState.RxRptr1 = headerBlock.Substring(8, 8).TrimEnd();
            clientState.RxUrCall = headerBlock.Substring(16, 8).TrimEnd();
            clientState.RxSrc = headerBlock.Substring(24, 8).TrimEnd();
        }
        else
        {
            logger.Debug($"Invalid packet: {Convert.ToHexString(packet)}");
        }
    }

    /// <summary>
    /// Decodes a 27-byte D-STAR voice frame, updates the client state, and extracts the 9-byte AMBE payload.
    /// </summary>
    /// <param name="packet">The raw 27-byte D-STAR voice frame to decode.</param>
    /// <param name="clientState">The client state object to update with stream information.</param>
    /// <returns>The 9-byte AMBE voice data extracted from the frame.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="packet"/> is not exactly 27 bytes long,
    /// or if it starts with the ASCII header "DSVT".
    /// </exception>
    internal static byte[] DecodeVoiceFrame(byte[] packet, DStarSessionContext clientState)
    {
        if (packet.Length != 27)
            throw new ArgumentException($"Invalid length of argument {nameof(packet)}, expecting 27, got {packet.Length}");

        if (Encoding.ASCII.GetString(packet, 0, 4) != "DSVT")
            throw new ArgumentException($"Invalid packet: {Convert.ToHexString(packet)}");

        clientState.RxStreamId = (ushort)(packet[12] << 8 | packet[13] & 0xFF);
        clientState.FrameNo = packet[14];

        SlowData.Decode(packet, 14, clientState);

        // Check for end of stream
        if ((packet[14] & 0x40) != 0)
        {
            clientState.RxStreamId = 0;
            clientState.RxStreamState = StreamState.End;
            logger.Debug($"RX stream ended, from {clientState.RxSrc} to {clientState.RxUrCall}");
        }

        return packet[15..24]; // 9 AMBE bytes
    }

}