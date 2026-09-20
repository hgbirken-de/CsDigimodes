using NLog;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace DigitalVoice.AmbeSupport;

/// <summary>
/// Implementation of a AMBE client. The client uses an AMBE server to send/receive data typically to decompress/compress AMBE data.
/// </summary>
public sealed class AmbeClient : IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const byte DV3000_START_BYTE = 0x61;
    private const byte DV3000_TYPE_CONTROL = 0x00;
    private const byte DV3000_CHANNEL_PACKET = 0x01;
    private const byte DV3000_SPEECH_PACKET = 0x02;

    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _remote;
    private bool _disposed;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="ip">the ip-address of the AMBE server</param>
    /// <param name="port">the port number of the AMBE server</param>
    /// <param name="timeoutMs">timeout value in millis</param>
    public AmbeClient(string ip = "127.0.0.1", int port = 2460, int timeoutMs = 1000)
    {
        logger.Debug($"ip={ip}, port={port}, timeout={timeoutMs}");
        _udpClient = new UdpClient();
        _udpClient.Client.ReceiveTimeout = timeoutMs;
        _remote = new IPEndPoint(IPAddress.Parse(ip), port);
    }

    /// <summary>
    /// Decode a single AMBE block (7 bytes) into 160 PCM samples (little‑endian shorts).
    /// </summary>
    /// <returns>[] on timeout or on unexpected packet type</returns>
    public short[] Decode(byte[] ambeData, int blockSize)
    {
        // build header
        int lengthField = blockSize + 2; // TODO: make sane

        byte[] packet = new byte[6 + ambeData.Length];
        packet[0] = DV3000_START_BYTE;
        packet[1] = (byte)(lengthField >> 8);
        packet[2] = (byte)(lengthField & 0xFF);
        packet[3] = DV3000_CHANNEL_PACKET;
        packet[4] = 0x01;             // field id: Compressed speech data to be decoded for current vocoder
        packet[5] = 0x31;             // bitCount = 49 here fix
        for (int i = 0; i< ambeData.Length; i++)
            packet[6+i] = ambeData[i];

        // send request
        _udpClient.Send(packet, packet.Length, _remote);

        // recv response
        try
        {
            var remoteEP = _remote;
            var packetRcvd = _udpClient.Receive(ref remoteEP);
            if (packetRcvd[3] != DV3000_SPEECH_PACKET) return [];

            int payloadLen = BinaryPrimitives.ReadUInt16BigEndian(packetRcvd.AsSpan(1, 2));
            //int payloadLen2 = packet.Length - 6;
            // payload begins at offset 6
            var payload = packetRcvd.AsSpan(6, payloadLen - 2);
            // (payloadLen includes bytes [4] + [5] so subtract those two to get raw PCM count)

            int sampleCount = payload.Length / 2;
            var pcm = new short[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                pcm[i] = BinaryPrimitives.ReadInt16BigEndian(payload.Slice(i + i, 2));
            }

            return pcm;
        }
        catch (SocketException ex)
        {
            logger.Error("Exception during read from AMBE server:", ex);
            return []; // timeout
        }
    }

    /// <summary>
    /// Encode 160 PCM samples (little‑endian shorts) into a single AMBE block.
    /// Returns the raw AMBE payload bytes (7 bytes for YSF).
    /// </summary>
    public byte[] Encode(short[] pcmSamples)
    {
        if (pcmSamples == null || pcmSamples.Length != 160)
            throw new ArgumentException($"expected 160 samples, got {pcmSamples?.Length}");

        // build header for 160 sample audio packet
        int payloadSize = 160 * 2;        // 320 bytes
        int totalSize = 6 + payloadSize;
        int lengthField = totalSize - 4;   // = 322 → 0x0142

        var packet = new byte[totalSize];
        packet[0] = DV3000_START_BYTE;
        packet[1] = (byte)(lengthField >> 8);
        packet[2] = (byte)(lengthField & 0xFF);
        packet[3] = DV3000_SPEECH_PACKET;
        packet[4] = 0x00;
        packet[5] = 0xA0; // 160 PCM samples

        // write PCM as big‑endian into cmd[6..]
        for (int i = 0; i < pcmSamples.Length; i++)
        {
            BinaryPrimitives.WriteInt16BigEndian(packet.AsSpan(6 + i + i, 2), pcmSamples[i]);
        }

        // send request
        _udpClient.Send(packet, packet.Length, _remote);

        // recv AMBE response
        var remoteEP = _remote;
        var resp = _udpClient.Receive(ref remoteEP);
        int respLen = BinaryPrimitives.ReadUInt16BigEndian(resp.AsSpan(1, 2));
        // AMBE payload = respLen - 2 bytes starting at offset 6
        int ambeLen = respLen - 2;
        var ambe = new byte[ambeLen];
        Array.Copy(resp, 6, ambe, 0, ambeLen);
        return ambe;
    }

    /// <summary>
    /// Blocking receive of PCM block; returns null on unexpected packet.
    /// </summary>
    public short[] ReceivePcm()
    {
        var remoteEP = _remote;
        var response = _udpClient.Receive(ref remoteEP);
        if (response[0] !=  DV3000_START_BYTE || response[3] != DV3000_SPEECH_PACKET) 
            return []; // invalid PCM data

        int payloadLen = BinaryPrimitives.ReadUInt16BigEndian(response.AsSpan(1, 2));
        int pcmBytes = payloadLen - 2;
        int sampleCount = pcmBytes / 2;
        var pcm = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
            pcm[i] = BinaryPrimitives.ReadInt16BigEndian(response.AsSpan(6 + i * 2, 2));

        return pcm;
    }

    /// <summary>
    /// Unsolicited send of AMBE block; response read later.
    /// </summary>
    public void SendAmbe(byte[] ambeData, int blockSize)
    {
        int lengthField = blockSize + 2;

        byte[] packet = new byte[6 + ambeData.Length];
        packet[0] = DV3000_START_BYTE;
        packet[1] = (byte)(lengthField >> 8);
        packet[2] = (byte)(lengthField & 0xFF);
        packet[3] = DV3000_CHANNEL_PACKET;
        packet[4] = 0x01;             // field id: Compressed speech data to be decoded for current vocoder
        packet[5] = 0x31;             // bitCount = 49 here fix
        for (int i = 0; i < ambeData.Length; i++)
            packet[6 + i] = ambeData[i];

        // send request
        _udpClient.Send(packet, packet.Length, _remote);
    }

    /// <summary>
    /// Sends a UDP packet to the configured remote endpoint and waits for a response.
    /// </summary>
    /// <param name="packet">The byte array containing the packet to send. Must be non-null, contain at least 5 bytes, and begin with the expected start byte.</param>
    /// <returns>The received response packet as a byte array, or <c>null</c> if a timeout occurs or another error prevents successful reception.</returns>
    /// <remarks>
    /// This method transmits the specified packet using the underlying <see cref="UdpClient"/> instance and then blocks until a response is received
    /// or the receive timeout elapses. Validation ensures the packet conforms to the DV3000 framing requirements.
    /// Timeout conditions are detected via <see cref="SocketException"/> with <see cref="SocketError.TimedOut"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="packet"/> is <c>null</c>. </exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="packet"/> is too short or does not begin with the expected start byte.</exception>
    public byte[]? SendReceivePacket(byte[] packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (packet.Length < 5)
            throw new ArgumentException("Packet must contain at least 5 bytes.", nameof(packet));

        if (packet[0] != DV3000_START_BYTE)
            throw new ArgumentException($"Packet does not start with the expected start byte: 0x{DV3000_START_BYTE:X2}.", nameof(packet));

        try
        {
            _udpClient.Send(packet, packet.Length, _remote);

            var remoteEP = _remote;
            return _udpClient.Receive(ref remoteEP);
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
        {
            logger.Error(ex, "UDP receive timed out.");
            return null;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to receive server packet.");
            return null;
        }
    }

    /// <summary>
    /// Sends a UDP packet to the configured remote endpoint.
    /// </summary>
    /// <param name="packet">The byte array containing the data to send. Must not be <c>null</c> and must contain at least one byte.</param>
    /// <remarks>
    /// This method uses the underlying <see cref="UdpClient"/> instance to transmit the packet to the remote endpoint configured for this server.</remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="packet"/> is <c>null</c>.</exception>
    /// <exception cref="SocketException">Thrown if an error occurs while sending the UDP packet.</exception>
    public void SendPacket(byte[] packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (packet.Length < 5)
            throw new ArgumentException("Packet must contain at least 5 bytes.", nameof(packet));

        if (packet[0] != DV3000_START_BYTE)
            throw new ArgumentException($"Packet does not start with the expected start byte: 0x{DV3000_START_BYTE:X2}.", nameof(packet));

        _udpClient.Send(packet, packet.Length, _remote);
    }


    /// <summary>
    /// Receives a single UDP packet from the configured remote endpoint.
    /// </summary>
    /// <returns>
    /// A byte array containing the received packet data, or <c>null</c> if a timeout occurs or an error prevents successful reception.
    /// </returns>
    /// <remarks>
    /// If a receive timeout is configured on the underlying <see cref="UdpClient"/>, this method catches <see cref="SocketException"/> 
    /// with <see cref="SocketError.TimedOut"/> and logs it as a timeout event. Any other exception is also logged and results in a <c>null</c> return value.
    /// </remarks>
    /// <exception cref="SocketException">Propagated only if the exception is not a timeout-related error.</exception>
    public byte[]? ReceivePacket()
    {
        try
        {
            var remoteEP = _remote;
            return _udpClient.Receive(ref remoteEP);
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
        {
            logger.Error(ex, "UDP receive timed out.");
            return null;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to receive server packet.");
            return null;
        }
    }


    public void Dispose()
    {
        if (!_disposed)
        {
            _udpClient.Close();
            _udpClient.Dispose();
            _disposed = true;
        }
    }
}

