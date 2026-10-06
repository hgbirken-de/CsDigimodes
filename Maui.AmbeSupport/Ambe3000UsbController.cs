using Android.Content;
using Android.Hardware.Usb;
using DigitalVoice.AmbeSupport;
using NLog;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Maui.AmbeSupport;

/// <summary>
/// AMBE3000R-USB-Stick (FTDI FT230X) über Android USB-Host/OTG als <see cref="IAmbe3000RController"/>.
/// <para>
/// Verhält sich wie der Windows-<c>Ambe3000RController</c>: synchron/blockierend, <see cref="ReceivePacket"/>
/// und <see cref="SendReceivePacket"/> liefern bei Timeout <c>null</c> (Gesamt-Obergrenze
/// <c>packetTimeout</c>, Standard 3 s) und werfen nicht. Der Zugriff muss vom Aufrufer serialisiert werden
/// (die Clients tun das mit ihrem <c>_chipLock</c>).
/// </para>
/// <para>
/// Android-Besonderheiten, die hier gekapselt sind: FTDI-Initialisierung per Vendor-Requests, die 2
/// Modem-Status-Bytes am Anfang JEDES USB-Pakets, kurzer FTDI-Latency-Timer (Standard wären 16 ms pro
/// Antwort) und ein eigener Empfangspuffer, weil USB-Paket- und DV3000-Paketgrenzen nicht zusammenfallen.
/// </para>
/// <para>
/// Die USB-Permission muss VOR <see cref="Open"/> vorliegen, siehe <see cref="AmbeUsb.RequestPermissionAsync"/>.
/// </para>
/// </summary>
public sealed class Ambe3000UsbController : IAmbe3000RController, IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // ---- DV3000-Protokoll ----
    private const byte StartByte = 0x61;
    private const byte TypeControl = 0x00;
    private const byte TypeChannel = 0x01; // AMBE-Daten
    private const byte TypeSpeech = 0x02;  // PCM
    private const byte CtrlProdId = 0x30;
    private const byte CtrlVersion = 0x31;
    private const byte CtrlReady = 0x39;
    private const int MaxPayload = 1024;   // alles darüber ist kein gültiger Header

    private static readonly byte[] ReqProdId = [StartByte, 0x00, 0x01, TypeControl, CtrlProdId];
    private static readonly byte[] ReqVersion = [StartByte, 0x00, 0x01, TypeControl, CtrlVersion];
    private static readonly byte[] ReqReset = [StartByte, 0x00, 0x07, TypeControl, 0x34, 0x05, 0x00, 0x00, 0x0F, 0x00, 0x00];

    // ---- FTDI-Vendor-Requests ----
    private const byte FtdiReqTypeOut = 0x40; // Vendor, Host-to-Device
    private const byte SioReset = 0x00;
    private const byte SioSetModemCtrl = 0x01;
    private const byte SioSetFlowCtrl = 0x02;
    private const byte SioSetBaudrate = 0x03;
    private const byte SioSetData = 0x04;
    private const byte SioSetLatencyTimer = 0x09;

    private const int WriteTimeoutMs = 1000;

    private readonly UsbManager _usbManager;
    private readonly UsbDevice _device;
    private readonly int _baudRate;
    private readonly int _packetTimeoutMs;
    private readonly int _latencyTimerMs;

    private readonly object _openCloseLock = new();

    private UsbDeviceConnection? _connection;
    private UsbInterface? _interface;
    private UsbEndpoint? _endpointIn;
    private UsbEndpoint? _endpointOut;

    // Ein Bulk-IN-Transfer liest genau EIN USB-Paket (= wMaxPacketSize): so beginnen die Daten garantiert
    // mit den 2 Modem-Status-Bytes eines einzigen Pakets, die wir abschneiden können.
    private byte[] _usbBuf = [];

    // Empfangspuffer: gültige Daten in _rx[_rxStart .. _rxEnd)
    private readonly byte[] _rx = new byte[2048];
    private int _rxStart;
    private int _rxEnd;

    /// <param name="context">Activity oder Application-Context.</param>
    /// <param name="device">Der USB-Stick (siehe <see cref="AmbeUsb.FindDevice"/>).</param>
    /// <param name="baudRate">Baudrate der FTDI-Seite (Standard 460800).</param>
    /// <param name="packetTimeout">Gesamt-Obergrenze in ms für den Empfang eines kompletten Pakets.</param>
    /// <param name="latencyTimerMs">FTDI-Latency-Timer in ms (1..255). Kleiner Wert = schnellere Antworten.</param>
    public Ambe3000UsbController(Context context, UsbDevice device, int baudRate = 460800,
        int packetTimeout = 3000, int latencyTimerMs = 2)
    {
        _usbManager = (UsbManager)context.GetSystemService(Context.UsbService)!;
        _device = device;
        _baudRate = baudRate;
        _packetTimeoutMs = packetTimeout;
        _latencyTimerMs = Math.Clamp(latencyTimerMs, 1, 255);

        logger.Debug($"device={device.DeviceName}, baudrate={baudRate}, packetTimeout={packetTimeout} ms, latencyTimer={_latencyTimerMs} ms");
    }

    // ------------------------------------------------------------------
    // Open / Close
    // ------------------------------------------------------------------

    public bool IsOpen => _connection != null;

    /// <summary>
    /// Öffnet die USB-Verbindung und initialisiert den FTDI-Chip. Idempotent. Kann nach <see cref="Close"/>
    /// erneut aufgerufen werden.
    /// </summary>
    /// <exception cref="InvalidOperationException">Keine USB-Permission.</exception>
    /// <exception cref="IOException">Gerät nicht erreichbar oder FTDI-Initialisierung fehlgeschlagen.</exception>
    public void Open()
    {
        lock (_openCloseLock)
        {
            if (_connection != null)
                return;

            if (_device.VendorId != AmbeUsb.FtdiVendorId)
                throw new IOException($"Unexpected vendor id 0x{_device.VendorId:X4} (expected FTDI 0x{AmbeUsb.FtdiVendorId:X4}).");

            if (!_usbManager.HasPermission(_device))
                throw new InvalidOperationException("No USB permission. Call AmbeUsb.RequestPermissionAsync() first.");

            UsbInterface usbInterface = FindInterface(_device)
                ?? throw new IOException("No suitable USB interface found (at least 2 endpoints expected).");
            UsbEndpoint endpointIn = FindEndpoint(usbInterface, UsbAddressing.In)
                ?? throw new IOException("Bulk IN endpoint not found.");
            UsbEndpoint endpointOut = FindEndpoint(usbInterface, UsbAddressing.Out)
                ?? throw new IOException("Bulk OUT endpoint not found.");

            UsbDeviceConnection connection = _usbManager.OpenDevice(_device)
                ?? throw new IOException("OpenDevice() failed (device busy or unreachable?).");

            if (!connection.ClaimInterface(usbInterface, true))
            {
                connection.Close();
                throw new IOException("ClaimInterface() failed.");
            }

            try
            {
                InitFtdi(connection, _baudRate);
            }
            catch
            {
                connection.ReleaseInterface(usbInterface);
                connection.Close();
                throw;
            }

            _usbBuf = new byte[Math.Max(endpointIn.MaxPacketSize, 4)];
            _rxStart = _rxEnd = 0;

            _interface = usbInterface;
            _endpointIn = endpointIn;
            _endpointOut = endpointOut;
            _connection = connection; // zuletzt: IsOpen wird erst jetzt true

            logger.Debug($"Opened {_device.DeviceName}, VID=0x{_device.VendorId:X4}, PID=0x{_device.ProductId:X4}, maxPacket={endpointIn.MaxPacketSize}");
        }
    }

    public void Close()
    {
        lock (_openCloseLock)
        {
            UsbDeviceConnection? connection = _connection;
            UsbInterface? usbInterface = _interface;

            _connection = null;
            _interface = null;
            _endpointIn = null;
            _endpointOut = null;
            _rxStart = _rxEnd = 0;

            if (connection == null)
                return;

            try
            {
                if (usbInterface != null)
                    connection.ReleaseInterface(usbInterface);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "ReleaseInterface failed.");
            }
            connection.Close();
            logger.Debug("Closed.");
        }
    }

    public void Dispose() => Close();

    private static UsbInterface? FindInterface(UsbDevice device)
    {
        for (int i = 0; i < device.InterfaceCount; i++)
        {
            UsbInterface usbInterface = device.GetInterface(i);
            if (usbInterface.EndpointCount >= 2)
                return usbInterface;
        }
        return null;
    }

    private static UsbEndpoint? FindEndpoint(UsbInterface usbInterface, UsbAddressing direction)
    {
        for (int i = 0; i < usbInterface.EndpointCount; i++)
        {
            UsbEndpoint endpoint = usbInterface.GetEndpoint(i);
            if (endpoint.Direction == direction && endpoint.Type == UsbAddressing.XferBulk)
                return endpoint;
        }
        return null;
    }

    private void InitFtdi(UsbDeviceConnection connection, int baudRate)
    {
        // Reset der FTDI-Seite (nicht zu verwechseln mit dem DV3000-Reset)
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioReset, 0x00, 0, null, 0, 1000), "SIO_RESET");

        var (value, index) = CalculateFtdiBaudDivisor(baudRate);
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetBaudrate, value, index, null, 0, 1000), "SIO_SET_BAUDRATE");

        // 8N1
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetData, 0x08, 0, null, 0, 1000), "SIO_SET_DATA");

        // DTR und RTS setzen (wValue: Bit0=DTR, Bit1=RTS, Bit8=DTR-Enable, Bit9=RTS-Enable).
        // HINWEIS: Der Windows-Controller setzt RtsEnable=false. Diese Android-Variante setzt RTS (wie pyserial
        // per Default). Falls sich beide Varianten auf demselben Stick unterschiedlich verhalten, hier angleichen.
        const int dtrHigh = 0x0101;
        const int rtsHigh = 0x0202;
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetModemCtrl, dtrHigh | rtsHigh, 0, null, 0, 1000), "SIO_SET_MODEM_CTRL");

        // Keine Flusskontrolle (wie Handshake.None unter Windows)
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetFlowCtrl, 0x00, 0, null, 0, 1000), "SIO_SET_FLOW_CTRL");

        // Latency-Timer verkürzen: Standard sind 16 ms. Der FTDI hält kurze Antworten (z.B. 13 Bytes) so lange
        // zurück, bis der Timer abläuft. Nicht kritisch, falls das fehlschlägt.
        int latency = connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetLatencyTimer, _latencyTimerMs, 0, null, 0, 1000);
        if (latency < 0)
            logger.Warn($"Setting the FTDI latency timer failed (result={latency}), keeping the default.");
    }

    private static void Check(int result, string step)
    {
        if (result < 0)
            throw new IOException($"FTDI control transfer '{step}' failed (result={result}).");
    }

    /// <summary>Berechnet Value/Index für SIO_SET_BAUDRATE (FT232R/FT230X-Standardformel; 9600 Baud ergibt 0x4138).</summary>
    private static (int value, int index) CalculateFtdiBaudDivisor(int baudRate)
    {
        const int ftdiClock = 3000000;
        int[] fracCode = [0, 3, 2, 4, 1, 5, 6, 7];

        int divisor8 = (ftdiClock * 8) / baudRate;
        int divisor = divisor8 >> 3;
        int fractionalOffset = fracCode[divisor8 & 0x7];

        int combined = divisor | (fractionalOffset << 14);
        if (combined == 1)
            combined = 0;
        else if (combined == 0x4001)
            combined = 1;

        return (combined & 0xFFFF, (combined >> 16) & 0xFFFF);
    }

    // ------------------------------------------------------------------
    // Rohes I/O
    // ------------------------------------------------------------------

    private void EnsureOpen()
    {
        if (_connection == null)
            throw new InvalidOperationException("AMBE USB device is not open.");
    }

    private void WriteRaw(byte[] data)
    {
        UsbDeviceConnection? connection = _connection;
        UsbEndpoint? endpoint = _endpointOut;
        if (connection == null || endpoint == null)
            throw new InvalidOperationException("AMBE USB device is not open.");

        int written = connection.BulkTransfer(endpoint, data, data.Length, WriteTimeoutMs);
        if (written != data.Length)
            throw new IOException($"USB write failed (result={written}, expected {data.Length}).");
    }

    private int RxCount => _rxEnd - _rxStart;

    private void ConsumeRx(int count)
    {
        _rxStart += count;
        if (_rxStart >= _rxEnd)
            _rxStart = _rxEnd = 0;
    }

    /// <summary>
    /// Liest EIN USB-Paket und hängt dessen Nutzdaten (ohne die 2 Modem-Status-Bytes) an den Empfangspuffer an.
    /// Kehrt bei Timeout, Fehler oder reinem Status-Paket ohne Daten zurück.
    /// </summary>
    private void FillRx(int timeoutMs)
    {
        UsbDeviceConnection? connection = _connection;
        UsbEndpoint? endpoint = _endpointIn;
        if (connection == null || endpoint == null)
            throw new InvalidOperationException("AMBE USB device is not open.");

        int n = connection.BulkTransfer(endpoint, _usbBuf, _usbBuf.Length, Math.Max(1, timeoutMs)); // 0 = unendlich, daher >= 1
        if (n <= 2)
            return; // Timeout/Fehler (-1) oder nur Modem-Status

        int payload = n - 2;
        if (_rxEnd + payload > _rx.Length)
        {
            int count = RxCount;
            Buffer.BlockCopy(_rx, _rxStart, _rx, 0, count);
            _rxStart = 0;
            _rxEnd = count;

            if (_rxEnd + payload > _rx.Length)
            {
                logger.Warn("RX buffer overflow, discarding buffered data.");
                _rxStart = _rxEnd = 0;
            }
        }

        Buffer.BlockCopy(_usbBuf, 2, _rx, _rxEnd, payload);
        _rxEnd += payload;
    }

    /// <summary>
    /// Liest das nächste vollständige DV3000-Paket (Startbyte 0x61 + 3 Header-Bytes + Nutzdaten).
    /// </summary>
    /// <exception cref="TimeoutException">Kein vollständiges Paket innerhalb von <paramref name="timeoutMs"/>.</exception>
    private byte[] ReadPacket(int timeoutMs)
    {
        long deadline = Environment.TickCount64 + timeoutMs;

        while (true)
        {
            // 1) Startbyte suchen, alles davor ist Müll
            while (RxCount > 0 && _rx[_rxStart] != StartByte)
                ConsumeRx(1);

            // 2) Header prüfen und Paket entnehmen
            if (RxCount >= 4)
            {
                int length = (_rx[_rxStart + 1] << 8) | _rx[_rxStart + 2];
                if (length > MaxPayload)
                {
                    ConsumeRx(1); // das 0x61 war nur Zufall in den Nutzdaten
                    continue;
                }

                int total = 4 + length;
                if (RxCount >= total)
                {
                    byte[] packet = new byte[total];
                    Buffer.BlockCopy(_rx, _rxStart, packet, 0, total);
                    ConsumeRx(total);
                    return packet;
                }
            }

            long remaining = deadline - Environment.TickCount64;
            if (remaining <= 0)
                throw new TimeoutException($"Timed out waiting for a chip response (packetTimeout={timeoutMs} ms).");

            FillRx((int)Math.Min(remaining, 20));
        }
    }

    // ------------------------------------------------------------------
    // IAmbe3000RController
    // ------------------------------------------------------------------

    public void SendPacket(byte[] packet)
    {
        EnsureOpen();
        WriteRaw(packet);
    }

    public byte[]? ReceivePacket()
    {
        EnsureOpen();
        try
        {
            return ReadPacket(_packetTimeoutMs);
        }
        catch (TimeoutException)
        {
            logger.Warn($"Timeout reading chip response (packetTimeout={_packetTimeoutMs} ms).");
            return null;
        }
    }

    public byte[]? SendReceivePacket(byte[] packet)
    {
        EnsureOpen();
        try
        {
            WriteRaw(packet);
            return ReadPacket(_packetTimeoutMs);
        }
        catch (TimeoutException)
        {
            logger.Warn("Timeout reading chip response.");
            return null;
        }
    }

    public async Task<byte[]?> SendReceivePacketAsync(byte[] packet, CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        try
        {
            return await Task.Run(() =>
            {
                WriteRaw(packet);
                return ReadPacket(_packetTimeoutMs);
            }, cancellationToken).ConfigureAwait(false);
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

    public string? GetProductId() => ReadControlString(ReqProdId, CtrlProdId, "product id");

    public string? GetVersion() => ReadControlString(ReqVersion, CtrlVersion, "version info");

    private string? ReadControlString(byte[] request, byte ctrlCode, string what)
    {
        EnsureOpen();
        try
        {
            WriteRaw(request);

            long deadline = Environment.TickCount64 + _packetTimeoutMs;
            while (true)
            {
                int remaining = (int)(deadline - Environment.TickCount64);
                if (remaining <= 0)
                    throw new TimeoutException();

                byte[] packet = ReadPacket(remaining);
                // unaufgeforderte Pakete (z.B. READY nach einem Reset) überspringen
                if (packet.Length > 5 && packet[3] == TypeControl && packet[4] == ctrlCode)
                    return Encoding.ASCII.GetString(packet, 5, packet.Length - 6).Trim(); // letztes Byte = 0x00-Terminator
            }
        }
        catch (TimeoutException)
        {
            logger.Warn($"Timeout reading {what}.");
            return null;
        }
    }

    /// <summary>
    /// Setzt den Vocoder per DV3000-Reset-Paket (PKT_RESETSOFTCFG) zurück und wartet auf "ready" (0x39).
    /// Anders als der Windows-Controller (sendet 0x90) ist das die auf dem Stick bereits erprobte Variante.
    /// </summary>
    /// <exception cref="TimeoutException">Der Chip meldet nicht "ready".</exception>
    public void Reset()
    {
        EnsureOpen();
        WriteRaw(ReqReset);

        long deadline = Environment.TickCount64 + _packetTimeoutMs;
        while (true)
        {
            int remaining = (int)(deadline - Environment.TickCount64);
            if (remaining <= 0)
                throw new TimeoutException("DV3000 did not report 'ready' after reset.");

            byte[] packet = ReadPacket(remaining);
            if (packet.Length > 4 && packet[3] == TypeControl && packet[4] == CtrlReady)
            {
                logger.Debug("DV3000 ready after reset.");
                return;
            }
        }
    }

    /// <summary>Wie beim Windows-Controller (Paketaufbau unverändert übernommen).</summary>
    public short[] Decode(byte[] ambeData, int blockSize)
    {
        EnsureOpen();

        int length = blockSize + 2;
        byte[] packet1 = new byte[6 + ambeData.Length];
        packet1[0] = StartByte;
        packet1[1] = 0x00;           // len MS
        packet1[2] = (byte)length;   // len LS
        packet1[3] = TypeChannel;
        packet1[4] = 0x01;           // field id: compressed speech data to be decoded
        packet1[5] = 0x31;           // number of bits (49)
        Buffer.BlockCopy(ambeData, 0, packet1, 6, ambeData.Length);

        WriteRaw(packet1);
        byte[] packet2 = ReadPacket(_packetTimeoutMs);

        // PCM kommt big endian -> little endian short[]
        int sampleCount = (packet2.Length - 6) / 2;
        var pcm = new short[sampleCount];
        ReadOnlySpan<short> bigEndian = MemoryMarshal.Cast<byte, short>(packet2.AsSpan(6, sampleCount * 2));
        for (int i = 0; i < sampleCount; i++)
            pcm[i] = BinaryPrimitives.ReverseEndianness(bigEndian[i]);

        return pcm;
    }

    /// <summary>Wie beim Windows-Controller (Paketaufbau unverändert übernommen).</summary>
    public byte[] Encode(short[] pcmSamples)
    {
        if (pcmSamples == null || pcmSamples.Length != 160)
            throw new ArgumentException($"Missing/invalid argument {nameof(pcmSamples)}", nameof(pcmSamples));
        EnsureOpen();

        byte[] cmd = new byte[6 + 160 * 2];
        cmd[0] = StartByte;
        cmd[1] = 0x01;               // MS-LEN
        cmd[2] = 0x42;               // LS-LEN (0x0142 = 322)
        cmd[3] = TypeSpeech;
        cmd[4] = 0x00;               // SPEECHD
        cmd[5] = 0xA0;               // sample count = 160

        Span<byte> payload = cmd.AsSpan(6);
        for (int i = 0; i < pcmSamples.Length; i++)
            BinaryPrimitives.WriteInt16BigEndian(payload.Slice(i * 2, 2), pcmSamples[i]);

        WriteRaw(cmd);
        byte[] packet = ReadPacket(_packetTimeoutMs);
        return packet[6..];
    }
}
