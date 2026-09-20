using DigitalVoice.Common;
using DigitalVoice.DStar.Common;
using NLog;
using System.Net.Sockets;
using System.Text;

namespace DigitalVoice.DStar.Dcs;

internal static class DcsCodec
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte[] EotAmbeFrame = { 0xdc, 0x8e, 0x0a, 0x40, 0xad, 0xed, 0xad, 0x39, 0x6e };

    static readonly byte[] txData = new byte[100];

    /// <summary>
    /// Decodes a 100-byte DCS voice frame, updates the client state, and extracts the 9-byte AMBE payload.
    /// </summary>
    /// <param name="packet">The raw 100-byte DCS frame to decode.</param>
    /// <param name="clientState">The client state object to update with stream information.</param>
    /// <returns>The 9-byte AMBE voice data extracted from the frame.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="packet"/> is not exactly 100 bytes long, or if it does not start with the ASCII prefix "0001".
    /// </exception>
    internal static byte[] DecodeDcs100Frame(byte[] packet, DStarSessionContext clientState)
    {
        if (packet.Length != 100)
            throw new ArgumentException($"Invalid arg {packet}, expecting 100 bytes, got {packet.Length}");

        if (Encoding.ASCII.GetString(packet, 0, 4) != "0001")
            throw new ArgumentException($"Invalid arg {packet}, no prefix '0001'");

        if (clientState.TransceiveMode == TransceiveMode.Rx && clientState.RxStreamId == 0)
        {
            clientState.RxStreamState = StreamState.New;
            clientState.RxStreamId = (ushort)(packet[43] << 8 | packet[44]);
            clientState.RxUsrMsg = string.Empty;

            // Decode the whole 32-byte block once
            string headerBlock = Encoding.ASCII.GetString(packet, 7, 32);

            // Extract fields (8 bytes each), trimming trailing spaces
            clientState.RxRptr2 = headerBlock[..8].TrimEnd();
            clientState.RxRptr1 = headerBlock.Substring(8, 8).TrimEnd();
            clientState.RxUrCall = headerBlock.Substring(16, 8).TrimEnd();
            clientState.RxSrc = headerBlock.Substring(24, 8).TrimEnd();
            logger.Debug($"New RX stream from {clientState.RxSrc} to {clientState.RxUrCall}");
            
            clientState.RxGpsData = string.Empty;
            clientState.RxUsrMsg = string.Empty;
        }
        else
        {
            clientState.RxStreamState = StreamState.Streaming;
        }

        clientState.FrameNo = packet[45];

        SlowData.Decode(packet, 45, clientState);

        // Check for stream end
        if ((packet[45] & 0x40) != 0)
        {
            clientState.RxStreamId = 0;
            clientState.RxStreamState = StreamState.End;
            logger.Debug($"RX stream ended, from {clientState.RxSrc} to {clientState.RxUrCall}");
        }

        return packet[46..55]; // AMBE data
    }

    /// <summary>
    /// Builds a complete DCS100 transmission frame containing callsigns, user message, stream ID, and one AMBE voice frame.  
    /// </summary>
    /// <param name="isLastFrame">Defines if the fram eto genarate is the last frame (end of transmission).</param>
    /// <param name="clientState">The client state with transmit context (callsigns, stream ID, counters).</param>
    /// <param name="ambeData">The 9-byte AMBE voice data block.</param>
    /// <returns>A byte array representing the encoded DCS100 frame.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="ambeData"/> is not 9 bytes long.</exception>
    internal static byte[] CreateDcs100Frame(bool isLastFrame, DStarSessionContext clientState, byte[] ambeData)
    {
        if (ambeData.Length != 9)
        {
            throw new ArgumentException($"Length of argument {nameof(ambeData)} is {ambeData.Length}, but 9 is required.");
        }

        Array.Clear(txData);

        // Prefix
        Encoding.ASCII.GetBytes("0001").CopyTo(txData, 0);

        // Callsigns
        Encoding.ASCII.GetBytes(clientState.TxRptr2.PadRight(8)).CopyTo(txData, 7);       // e.g. 'DCS003 Z'
        Encoding.ASCII.GetBytes(clientState.TxRptr1.PadRight(8)).CopyTo(txData, 7 + 8);   // e.g. 'DL1HGB C'
        Encoding.ASCII.GetBytes(clientState.TxUrCall.PadRight(8)).CopyTo(txData, 7 + 16); // e.g. 'CQCQCQ  '
        Encoding.ASCII.GetBytes(clientState.TxMyCall.PadRight(8)).CopyTo(txData, 7 + 24); // e.g. 'DL1HGB  '

        Encoding.ASCII.GetBytes("AMBE").CopyTo(txData, 39);

        // TX stream
        if (clientState.TxStreamId == 0)
        {
            clientState.TxStreamId = (ushort)(Random.Shared.Next() & 0xFFFF);
        }
        txData[43] = (byte)(clientState.TxStreamId >> 8 & 0xFF);
        txData[44] = (byte)(clientState.TxStreamId & 0xFF);

        // Frame number
        txData[45] = (byte)(clientState.TxFrameCnt % 21);

        // SD User message
        SlowData.EncodeUsrMsg(txData, 45, clientState);

        if (isLastFrame)
        {
            // Mark this frame as the last one in the stream
            txData[45] |= 0x40;

            // Insert the special EOT AMBE frame
            EotAmbeFrame.CopyTo(txData, 46);
        }
        else
        {
            ambeData.CopyTo(txData, 46);
        }

        return txData;
    } 
}