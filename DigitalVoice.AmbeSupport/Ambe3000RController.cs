using NLog;
using System.Buffers.Binary;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;

namespace DigitalVoice.AmbeSupport;

/// <summary>
/// Class to access an AMBE3000R stick (chip).
/// </summary>
public class Ambe3000RController : IAmbe3000RController
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte DV3000_START_BYTE = 0x61;

    static readonly byte DV3000_CONTROL_PACKET = 0x00;
    static readonly byte DV3000_CHANNEL_PACKET = 0x01;
    static readonly byte DV3000_SPEECH_PACKET = 0x02;

    static readonly byte DV3000_TYPE_CONTROL = 0x00;
    static readonly byte DV3000_TYPE_AMBE = 0x01;
    static readonly byte DV3000_TYPE_AUDIO = 0x02;

    readonly string _portName;
    readonly int _baudRate;
    readonly int _timeout; // milliseconds
    SerialPort? serialPort;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portName"></param>
    /// <param name="baudRate"></param>
    /// <param name="timeout"></param>
    public Ambe3000RController(string portName, int baudRate = 460800, int timeout = 1000)
    {
        _portName = portName;
        _baudRate = baudRate;
        _timeout = timeout;

        logger.Debug($"port={portName}, baudrate={baudRate}, timeout={timeout} ms");
    }

    /// <summary>
    /// Opens the serial port.
    /// </summary>
    public void Open()
    {
        logger.Debug($"portName={_portName}, baudRate={_baudRate}");
        if (IsOpen)
            return;

        serialPort = new SerialPort(_portName, _baudRate)
        {
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            RtsEnable = false,
            DtrEnable = true,
            ReadTimeout = _timeout,
            WriteTimeout = -1,
            WriteBufferSize = 512,
            ReadBufferSize = 512,
        };
        serialPort.Open();
    }

    /// <summary>
    /// Close the serial port.
    /// </summary>
    public void Close()
    {
        logger.Debug(""); 
        serialPort?.Close();
        serialPort = null;
    }

    /// <summary>
    /// Check if the serial port is available (i.e. usable).
    /// </summary>
    /// <returns>true if available, otherwise false</returns>
    public bool IsOpen => serialPort != null && serialPort.IsOpen;

    /// <summary>
    /// Decode an AMBE data block.
    /// </summary>
    /// <param name="ambeData"></param>
    /// <param name="blockSize">length of the AMBE data block (e.g. 7)</param>
    /// <returns>PCM data as an array of short (little endian)</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public short[] Decode(byte[] ambeData, int blockSize)
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");

        int length = blockSize + 2; // should always be 9 for YSF

        byte[] packet1 = new byte[6 + ambeData.Length];
        packet1[0] = DV3000_START_BYTE;
        packet1[1] = 0x00;           // len MS
        packet1[2] = (byte)length;   // len LS
        packet1[3] = DV3000_CHANNEL_PACKET;
        packet1[4] = 0x01;           // field id: Compressed speech data to be decoded for current vocoder
        packet1[5] = 0x31;           // number of bits (49)
        for (int i = 0; i < ambeData.Length; i++)
            packet1[i + 6] = ambeData[i];

        serialPort!.Write(packet1, 0, packet1.Length);
        byte[] packet2 = ReadPacketFromPort();

        // Convert big‑endian 16‑bit samples into little‑endian short[]
        //short len = (short)(packet[1] << 8 | packet[2]); // length field of packet according to DVSI specification 
        int payloadLen = packet2.Length - 6; // 6 = header length
        int sampleCount = payloadLen / 2;
        var pcm = new short[sampleCount];
        int payloadOffset = 6;

        // reinterpret the payload bytes as big‑endian Int16
        ReadOnlySpan<byte> payloadBytes = packet2.AsSpan(payloadOffset);
        ReadOnlySpan<short> bigEndianSamples = MemoryMarshal.Cast<byte, short>(payloadBytes);

        // now swap to little‑endian if needed
        for (int i = 0; i < sampleCount; i++)
        {
            pcm[i] = BinaryPrimitives.ReverseEndianness(bigEndianSamples[i]); // hardware‐accelerated byte‑swap
        }

        return pcm;
    }

    /// <summary>
    /// Encode PCM samples to AMBE.
    /// </summary>
    /// <param name="pcm_samples"></param>
    /// <returns>an byte array containing the AMBE data</returns>
    /// <exception cref="ArgumentException"></exception>
    public byte[] Encode(short[] pcm_samples)
    {
        if (pcm_samples == null || pcm_samples.Length != 160)
            throw new ArgumentException($"Missing/invalid argument {nameof(pcm_samples)}");

        // total packet size = 6‑byte header + 160 * 2 bytes of PCM
        byte[] cmd = new byte[6 + (160 * 2)];

        // --- build header ---
        cmd[0] = DV3000_START_BYTE;
        cmd[1] = 0x01;               // MS‑LEN
        cmd[2] = 0x42;               // LS‑LEN (0x0142 = 322 bytes payload)
        cmd[3] = DV3000_SPEECH_PACKET;
        cmd[4] = 0x00;               // SPEECHD
        cmd[5] = 0xA0;               // sample Count = 160

        // --- write PCM samples as big‑endian directly into the array ---
        // payload starts at offset 6
        Span<byte> payload = cmd.AsSpan(6);
        for (int i = 0; i < pcm_samples.Length; i++)
        {
            // writes two bytes in big‑endian order
            BinaryPrimitives.WriteInt16BigEndian(payload.Slice(i + i, 2), pcm_samples[i]);
        }

        // -  send it AMBE3000R stick
        serialPort!.Write(cmd.ToArray(), 0, cmd.Length);
        
        byte[] packet = ReadPacketFromPort();

        return packet[6..]; // return the AMBE block
    }

    /// <summary>
    /// Read the product indentification of the AMBE3000R stick. 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string? GetProductId()
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");

        try
        { 
            byte[] cmd = [0x61, 0x00, 0x01, 0x00, 0x30];
            serialPort?.Write(cmd, 0, cmd.Length);
            byte[] buffer = ReadPacketFromPort();
            if (buffer.Length > 0)
            {
                return Encoding.ASCII.GetString(buffer, 5, buffer.Length - 6).Trim(); // igoring the 0x00 terminator
            }
        }
        catch (TimeoutException)
        {
            logger.Warn("Timeout reading product id.");
        }
        return null;
    }

    /// <summary>
    /// Read the version of the AMBE3000R stick 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string? GetVersion()
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");

        try
        { 
            byte[] cmd = [0x61, 0x00, 0x01, 0x00, 0x31];
            serialPort?.Write(cmd, 0, cmd.Length);
            byte[] buffer = ReadPacketFromPort();

            if (buffer.Length > 0)
            {
                return Encoding.ASCII.GetString(buffer, 5, buffer.Length-6).Trim(); // igoring the 0x00 terminator
            }
        }
        catch (TimeoutException)
        {
            logger.Warn("Timeout reading version info.");
        }
        return null;
    }

    /// <summary>
    /// Read a packet from the serial port.
    /// </summary>
    /// <returns>The packet read.</returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public byte[] ReadPacketFromPort()
    {
        if (!IsOpen)
            throw new InvalidOperationException("Serial port is not open.");

        // ---- 1) Find the start byte ----
        byte start;
        while (true)
        {
            int b;
            try
            {
                b = serialPort!.ReadByte(); // blocks until data or timeout
            }
            catch (TimeoutException)
            {
                continue; // just loop again, no packet yet
            }

            if (b == -1)
                continue;

            if ((byte)b == 0x61)
            {
                start = (byte)b;
                break;
            }

            // ignore garbage bytes and keep looking
        }

        // ---- 2) Read remaining header bytes ----
        byte[] header = new byte[4];
        header[0] = start;

        int offset = 1;
        while (offset < 4)
        {
            try
            {
                int read = serialPort.Read(header, offset, 4 - offset);
                if (read > 0)
                {
                    offset += read;
                }
            }
            catch (TimeoutException)
            {
                // If no data, loop again — do NOT lose the already-read bytes
                continue;
            }
        }

        int length = (header[1] << 8) | header[2];

        // ---- 3) Read payload ----
        byte[] payload = new byte[length];
        offset = 0;

        while (offset < length)
        {
            try
            {
                int read = serialPort.Read(payload, offset, length - offset);
                if (read > 0)
                {
                    offset += read;
                }
            }
            catch (TimeoutException)
            {
                // Allow timeouts — continue until payload is complete
                continue;
            }
        }

        // ---- 4) Combine header + payload ----
        byte[] packet = new byte[4 + length];
        Buffer.BlockCopy(header, 0, packet, 0, 4);
        Buffer.BlockCopy(payload, 0, packet, 4, length);

        return packet;
    }

    public void Reset()
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");

        byte[] resetCmd = [0x90];
        serialPort!.Write(resetCmd, 0, resetCmd.Length);
        logger.Debug("Sent reset command (0x90).");
    }

    /// <summary>
    /// Sends a packet to the chip and receives the chip response synchronously. No data processing, both packets are passed-through.
    /// </summary>
    /// <param name="packet">The packet to be send to the chip.</param>
    /// <returns>The packet received from the chip.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public byte[]? SendReceivePacket(byte[] packet)
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");
        try
        {
            serialPort!.Write(packet, 0, packet.Length);            
            byte[] resp = ReadPacketFromPort();
            return resp;
        }
        catch (TimeoutException)
        {
            logger.Warn("Timeout reading chip response.");
        }
        return null;
    }

    public void SendPacket(byte[] packet)
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");
        try
        {
            serialPort!.Write(packet, 0, packet.Length);
        }
        catch (TimeoutException)
        {
            logger.Warn("Timeout reading chip response.");
        }
    }

    public byte[]? ReceivePacket()
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");
        try
        {
            byte[] resp = ReadPacketFromPort();
            return resp;
        }
        catch (TimeoutException)
        {
            //logger.Warn("Timeout reading chip response.");
        }
        return null;
    }


    /// <summary>
    /// Sends a packet to the chip and receives the chip response asynchronously. No data processing, both packets are passed-through.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<byte[]?> SendReceivePacketAsync(byte[] packet, CancellationToken cancellationToken = default)
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open.");

        try
        {
            return await Task.Run(() =>
            {
                serialPort!.Write(packet, 0, packet.Length);
                return ReadPacketFromPort();
            }, cancellationToken);
        }
        catch (TimeoutException)
        {
            logger.Warn("Timeout reading chip response.");
            return null;
        }
        catch (OperationCanceledException)
        {
            logger.Warn("Method/task canceled.");
            return null;
        }
    }

}