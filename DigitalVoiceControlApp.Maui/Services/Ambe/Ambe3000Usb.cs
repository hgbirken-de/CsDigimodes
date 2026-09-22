using Android.App;
using Android.Content;
using Android.Hardware.Usb;

namespace DigitalVoiceControlApp.Maui.Services.Ambe;

/// <summary>
/// Ansteuerung eines AMBE3000R-USB-Boards (FTDI-basiert) über Android USB-Host/OTG.
/// Kapselt: Permission-Request-Flow, FTDI-Chip-Initialisierung (Reset/Baudrate/Line-Control),
/// Stripping der FTDI-Modem-Status-Bytes bei Bulk-IN-Transfers, DV3000-Protokoll-Framing, und async I/O.
/// </summary>
public sealed class Ambe3000Usb : IDisposable
{
    // ---- FTDI Vendor-Request-Konstanten ----
    private const int FtdiVendorId = 0x0403;

    private const byte FtdiReqTypeOut = 0x40; // Vendor, Host-to-Device
    private const byte SioReset = 0x00;
    private const byte SioSetModemCtrl = 0x01;
    private const byte SioSetFlowCtrl = 0x02;
    private const byte SioSetBaudrate = 0x03;
    private const byte SioSetData = 0x04;

    // Abgeglichen mit der bestehenden Python-Referenzimplementierung (pyserial, DV3000Controller)
    // und den übrigen Sprachimplementierungen (C++, Kotlin, ...).
    private const int DefaultBaudRate = 460800;

    private const string ActionUsbPermission = "com.digitalvoicecontrolapp.USB_PERMISSION";

    private readonly UsbManager _usbManager;
    private readonly Context _context;

    private UsbDevice? _device;
    private UsbDeviceConnection? _connection;
    private UsbInterface? _interface;
    private UsbEndpoint? _endpointIn;
    private UsbEndpoint? _endpointOut;

    private UsbPermissionReceiver? _permissionReceiver;
    private TaskCompletionSource<bool>? _permissionTcs;

    private readonly SemaphoreSlim _ioLock = new(1, 1);

    public bool IsOpen => _connection != null;

    public event Action<string>? Log;

    public Ambe3000Usb(Context context)
    {
        _context = context;
        _usbManager = (UsbManager)context.GetSystemService(Context.UsbService)!;
    }

    // ------------------------------------------------------------------
    // Permission-Flow
    // ------------------------------------------------------------------

    /// <summary>
    /// Fordert die USB-Permission für das Gerät an, falls noch nicht erteilt.
    /// Muss vor Open() aufgerufen werden, wenn HasPermission() false liefert.
    /// </summary>
    public Task<bool> RequestPermissionAsync(UsbDevice device)
    {
        if (_usbManager.HasPermission(device))
            return Task.FromResult(true);

        _permissionTcs = new TaskCompletionSource<bool>();

        _permissionReceiver = new UsbPermissionReceiver(granted =>
        {
            _permissionTcs?.TrySetResult(granted);
        });

        var filter = new IntentFilter(ActionUsbPermission);

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            _context.RegisterReceiver(_permissionReceiver, filter, ReceiverFlags.NotExported);
        }
        else
        {
            _context.RegisterReceiver(_permissionReceiver, filter);
        }

        var flags = OperatingSystem.IsAndroidVersionAtLeast(31)
            ? PendingIntentFlags.Mutable
            : PendingIntentFlags.UpdateCurrent;

        var intent = new Intent(ActionUsbPermission);
        // WICHTIG: Package explizit setzen, sonst wird der Broadcast auf Android 14+
        // ggf. nicht zugestellt (implicit broadcast restrictions).
        intent.SetPackage(_context.PackageName);

        var pendingIntent = PendingIntent.GetBroadcast(_context, 0, intent, flags);

        _usbManager.RequestPermission(device, pendingIntent);

        return _permissionTcs.Task;
    }

    private sealed class UsbPermissionReceiver : BroadcastReceiver
    {
        private readonly Action<bool> _callback;

        public UsbPermissionReceiver(Action<bool> callback) => _callback = callback;

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent?.Action != ActionUsbPermission)
                return;

            bool granted = intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false);
            _callback(granted);
        }
    }

    // ------------------------------------------------------------------
    // Open / Close
    // ------------------------------------------------------------------

    /// <summary>
    /// Öffnet die Verbindung zum AMBE3000R. Erwartet, dass HasPermission() bereits true ist
    /// (ggf. vorher RequestPermissionAsync() awaiten).
    /// </summary>
    public bool Open(UsbDevice device, out string? failReason)
    {
        failReason = null;

        if (device.VendorId != FtdiVendorId)
        {
            failReason = $"Unerwartete Vendor-ID 0x{device.VendorId:X4} (erwartet 0x{FtdiVendorId:X4} = FTDI).";
            return false;
        }

        if (!_usbManager.HasPermission(device))
        {
            failReason = "Keine USB-Permission. RequestPermissionAsync() vorher awaiten.";
            return false;
        }

        var usbInterface = FindInterface(device);
        if (usbInterface == null)
        {
            failReason = "Kein passendes USB-Interface gefunden (mind. 2 Endpoints erwartet).";
            return false;
        }

        var endpointIn = FindEndpoint(usbInterface, UsbAddressing.In);
        var endpointOut = FindEndpoint(usbInterface, UsbAddressing.Out);

        if (endpointIn == null || endpointOut == null)
        {
            failReason = "Bulk-IN oder Bulk-OUT Endpoint nicht gefunden.";
            return false;
        }

        var connection = _usbManager.OpenDevice(device);
        if (connection == null)
        {
            failReason = "OpenDevice() fehlgeschlagen (Gerät belegt oder nicht erreichbar?).";
            return false;
        }

        if (!connection.ClaimInterface(usbInterface, true))
        {
            connection.Close();
            failReason = "ClaimInterface() fehlgeschlagen.";
            return false;
        }

        _device = device;
        _connection = connection;
        _interface = usbInterface;
        _endpointIn = endpointIn;
        _endpointOut = endpointOut;

        try
        {
            InitFtdi(connection, DefaultBaudRate);
        }
        catch (Exception ex)
        {
            failReason = $"FTDI-Initialisierung fehlgeschlagen: {ex.Message}";
            Close();
            return false;
        }

        Log?.Invoke($"AMBE3000 geöffnet: {device.DeviceName}, VID=0x{device.VendorId:X4}, PID=0x{device.ProductId:X4}");
        return true;
    }

    private static UsbInterface? FindInterface(UsbDevice device)
    {
        for (int i = 0; i < device.InterfaceCount; i++)
        {
            var usbInterface = device.GetInterface(i);

            if (usbInterface.EndpointCount >= 2)
                return usbInterface;
        }

        return null;
    }

    private static UsbEndpoint? FindEndpoint(UsbInterface usbInterface, UsbAddressing direction)
    {
        for (int i = 0; i < usbInterface.EndpointCount; i++)
        {
            var endpoint = usbInterface.GetEndpoint(i);

            if (endpoint.Direction == direction && endpoint.Type == UsbAddressing.XferBulk)
            {
                return endpoint;
            }
        }

        return null;
    }

    public void Close()
    {
        if (_connection != null && _interface != null)
        {
            _connection.ReleaseInterface(_interface);
        }

        _connection?.Close();

        _connection = null;
        _interface = null;
        _endpointIn = null;
        _endpointOut = null;
        _device = null;

        if (_permissionReceiver != null)
        {
            try { _context.UnregisterReceiver(_permissionReceiver); }
            catch (Java.Lang.IllegalArgumentException) { /* war nicht (mehr) registriert */ }

            _permissionReceiver = null;
        }
    }

    public void Dispose()
    {
        Close();
        _ioLock.Dispose();
    }

    // ------------------------------------------------------------------
    // FTDI-Initialisierung
    // ------------------------------------------------------------------

    private void InitFtdi(UsbDeviceConnection connection, int baudRate)
    {
        // Reset (FTDI-Chip-Ebene, nicht zu verwechseln mit dem DV3000-Protokoll-Reset weiter unten)
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioReset, 0x00, 0, null, 0, 1000), "SIO_RESET");

        // Baudrate
        var (value, index) = CalculateFtdiBaudDivisor(baudRate);
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetBaudrate, value, index, null, 0, 1000), "SIO_SET_BAUDRATE");

        // 8N1: Bits 0-7 = Datenbits (8), Bit 8-10 = Parität (0=None), Bit 11-12 = Stopbits (0=1 Stopbit)
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetData, 0x08, 0, null, 0, 1000), "SIO_SET_DATA");

        // DTR und RTS setzen (High). pyserial macht das beim Öffnen automatisch (Default),
        // unsere rohe Implementierung bisher nicht. Auf vielen FTDI-Boards ist DTR/RTS als
        // Reset-/Enable-Leitung zum eigentlichen Zielchip (hier: AMBE3000) verdrahtet – ohne
        // diesen Schritt bleibt der Chip im Reset und antwortet auf nichts.
        // wValue: Bit0=DTR-Zustand, Bit1=RTS-Zustand, Bit8=DTR-Enable, Bit9=RTS-Enable.
        const int dtrHigh = 0x0101;
        const int rtsHigh = 0x0202;
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetModemCtrl, dtrHigh | rtsHigh, 0, null, 0, 1000), "SIO_SET_MODEM_CTRL (DTR+RTS high)");

        // Flow Control aus. Laut DVSI-Manual gibt der AMBE-Chip über RTSn (Pin 64) an, wann sein
        // Empfangspuffer voll ist; ob das genutzt werden kann, hängt davon ab, ob RTSn auf dem
        // jeweiligen Stick mit dem FTDI-CTS#-Pin verdrahtet ist (Board-Design, nicht Chip-Spezifikation).
        // Falls ja und Hardware-Flow-Control gewünscht: hier 0x0100 (RTS/CTS) statt 0x00 verwenden.
        Check(connection.ControlTransfer((UsbAddressing)FtdiReqTypeOut, SioSetFlowCtrl, 0x00, 0, null, 0, 1000), "SIO_SET_FLOW_CTRL");
    }

    private static void Check(int result, string step)
    {
        if (result < 0)
            throw new IOException($"FTDI ControlTransfer '{step}' fehlgeschlagen (Ergebnis={result}).");
    }

    /// <summary>
    /// Berechnet Value/Index für SIO_SET_BAUDRATE nach FTDI-Standardformel (FT232R/FT230X-kompatibel).
    /// Gegenprobe: für 9600 Baud muss value = 0x4138 herauskommen (bekannter FTDI-Referenzwert).
    /// Für FT2232H/FT4232H-Chips mit High-Speed-Modus können abweichende Divisoren nötig sein.
    /// </summary>
    private static (int value, int index) CalculateFtdiBaudDivisor(int baudRate)
    {
        const int ftdiClock = 3000000; // 3 MHz Referenztakt

        int[] fracCode = { 0, 3, 2, 4, 1, 5, 6, 7 };

        // WICHTIG: zuerst mit 8 multiplizieren, DANN runden – sonst geht der Bruchteil-Anteil
        // verloren, bevor er in den fracCode-Index einfließen kann (das war der ursprüngliche Bug).
        int divisor8 = (ftdiClock * 8) / baudRate;

        int divisor = divisor8 >> 3;
        int fractionIndex = divisor8 & 0x7;
        int fractionalOffset = fracCode[fractionIndex];

        int combined = divisor | (fractionalOffset << 14);

        if (combined == 1)
            combined = 0;
        else if (combined == 0x4001)
            combined = 1;

        int value = combined & 0xFFFF;
        int index = (combined >> 16) & 0xFFFF;

        return (value, index);
    }

    // ------------------------------------------------------------------
    // Rohes I/O
    // ------------------------------------------------------------------

    /// <summary>
    /// Schreibt Daten an das AMBE3000. Thread-safe.
    /// </summary>
    public async Task<int> WriteAsync(byte[] data, int timeout = 1000, CancellationToken ct = default)
    {
        if (_connection == null || _endpointOut == null)
            throw new InvalidOperationException("AMBE USB device is not open.");

        await _ioLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await Task.Run(
                () => _connection.BulkTransfer(_endpointOut, data, data.Length, timeout),
                ct).ConfigureAwait(false);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// Liest Daten vom AMBE3000. Entfernt automatisch die 2 FTDI-Modem-Status-Bytes,
    /// die jedem Bulk-IN-Paket vorangestellt sind. Thread-safe.
    /// </summary>
    public async Task<int> ReadAsync(byte[] buffer, int timeout = 1000, CancellationToken ct = default)
    {
        if (_connection == null || _endpointIn == null)
            throw new InvalidOperationException("AMBE USB device is not open.");

        await _ioLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // +2 für die FTDI-Modem-Status-Bytes, die jedem Bulk-IN-Paket vorangestellt sind
            var raw = new byte[buffer.Length + 2];

            int n = await Task.Run(
                () => _connection.BulkTransfer(_endpointIn, raw, raw.Length, timeout),
                ct).ConfigureAwait(false);

            if (n <= 2)
                return 0;

            int payloadLen = n - 2;
            Array.Copy(raw, 2, buffer, 0, payloadLen);
            return payloadLen;
        }
        finally
        {
            _ioLock.Release();
        }
    }

    // Synchrone Varianten beibehalten, falls Aufrufstellen (noch) nicht async sind.
    // ACHTUNG: Nicht vom UI-Thread aus aufrufen – blockiert bis Timeout.
    public int Write(byte[] data, int timeout = 1000) => WriteAsync(data, timeout).GetAwaiter().GetResult();

    public int Read(byte[] buffer, int timeout = 1000) => ReadAsync(buffer, timeout).GetAwaiter().GetResult();

    // ------------------------------------------------------------------
    // AMBE3000/DV3000-Protokoll-Layer
    //
    // Portiert aus der bestehenden Python-Referenzimplementierung (DV3000Controller).
    // Wichtig: Anders als bei pyserial (VCP-Treiber übernimmt Framing-Pufferung)
    // müssen wir hier selbst einen kleinen Empfangspuffer über mehrere
    // Bulk-IN-Transfers hinweg verwalten, da ein einzelnes USB-Paket nicht
    // zwingend mit einer AMBE-Paketgrenze zusammenfällt.
    // ------------------------------------------------------------------

    private const byte DV3000_START_BYTE = 0x61;

    private const byte DV3000_TYPE_CONTROL = 0x00;
    private const byte DV3000_TYPE_AMBE = 0x01;
    private const byte DV3000_TYPE_AUDIO = 0x02;

    private const byte DV3000_CONTROL_PRODID = 0x30;
    private const byte DV3000_CONTROL_VERSTRING = 0x31;
    private const byte DV3000_CONTROL_READY = 0x39;

    private static readonly byte[] DV3000_REQ_PRODID =
        { DV3000_START_BYTE, 0x00, 0x01, DV3000_TYPE_CONTROL, DV3000_CONTROL_PRODID };

    private static readonly byte[] DV3000_REQ_VERSTRING =
        { DV3000_START_BYTE, 0x00, 0x01, DV3000_TYPE_CONTROL, DV3000_CONTROL_VERSTRING };

    private static readonly byte[] DV3000_REQ_RESET =
        { DV3000_START_BYTE, 0x00, 0x07, DV3000_TYPE_CONTROL, 0x34, 0x05, 0x00, 0x00, 0x0F, 0x00, 0x00 };

    private readonly Queue<byte> _rxBuffer = new();

    private async Task<byte[]> ReadExactAsync(int n, int timeoutMs, CancellationToken ct)
    {
        var result = new byte[n];
        int got = 0;
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (got < n)
        {
            if (_rxBuffer.Count == 0)
            {
                if (DateTime.UtcNow > deadline)
                    throw new TimeoutException($"Timeout beim Lesen von {n} Bytes (erhalten: {got}).");

                var chunk = new byte[64]; // typische FTDI-Paketgröße
                int remainingMs = Math.Max(20, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
                int read = await ReadAsync(chunk, Math.Min(remainingMs, 200), ct).ConfigureAwait(false);

                if (read == 0)
                {
                    await Task.Delay(10, ct).ConfigureAwait(false);
                    continue;
                }

                for (int i = 0; i < read; i++)
                    _rxBuffer.Enqueue(chunk[i]);
            }

            result[got++] = _rxBuffer.Dequeue();
        }

        return result;
    }

    /// <summary>
    /// Liest und parst das nächste AMBE3000/DV3000-Antwortpaket.
    /// </summary>
    public async Task<(byte? RespType, byte? CtrlCode, byte[] Packet)> GetResponseAsync(
        int timeoutMs = 3000, CancellationToken ct = default)
    {
        var first = await ReadExactAsync(1, timeoutMs, ct).ConfigureAwait(false);
        if (first[0] != DV3000_START_BYTE)
        {
            Log?.Invoke($"Unerwartetes Start-Byte: 0x{first[0]:X2}, verworfen.");
            return (null, null, Array.Empty<byte>());
        }

        var header = await ReadExactAsync(3, timeoutMs, ct).ConfigureAwait(false);
        int lenField = (header[0] & 0x0F) * 256 + header[1] + 4;

        var rest = await ReadExactAsync(lenField - 4, timeoutMs, ct).ConfigureAwait(false);

        var packet = new byte[first.Length + header.Length + rest.Length];
        Buffer.BlockCopy(first, 0, packet, 0, first.Length);
        Buffer.BlockCopy(header, 0, packet, first.Length, header.Length);
        Buffer.BlockCopy(rest, 0, packet, first.Length + header.Length, rest.Length);

        byte respType = packet[3];
        byte? ctrlCode = respType == DV3000_TYPE_CONTROL ? packet[4] : null;

        return (respType, ctrlCode, packet);
    }

    /// <summary>
    /// Sendet das DV3000-Reset-Kommando (Protokoll-Ebene, nicht zu verwechseln mit
    /// dem FTDI-SIO_RESET aus InitFtdi) und wartet auf die Ready-Antwort.
    /// Guter End-to-End-Test: schlägt fehl, wenn FTDI-Ebene zwar offen ist,
    /// der DV3000-Chip aber nicht korrekt antwortet.
    /// </summary>
    public async Task ResetVocoderAsync(int timeoutMs = 3000, CancellationToken ct = default)
    {
        await WriteAsync(DV3000_REQ_RESET, timeoutMs, ct).ConfigureAwait(false);

        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            int remaining = Math.Max(100, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
            var (t, c, _) = await GetResponseAsync(remaining, ct).ConfigureAwait(false);

            if (t == DV3000_TYPE_CONTROL && c == DV3000_CONTROL_READY)
            {
                Log?.Invoke("DV3000 ready nach Reset.");
                return;
            }
        }

        throw new TimeoutException("DV3000 hat nach Reset nicht 'ready' gemeldet.");
    }

    public async Task<string> GetProductIdAsync(int timeoutMs = 3000, CancellationToken ct = default)
    {
        await WriteAsync(DV3000_REQ_PRODID, timeoutMs, ct).ConfigureAwait(false);

        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            int remaining = Math.Max(100, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
            var (t, c, pkt) = await GetResponseAsync(remaining, ct).ConfigureAwait(false);

            if (t == DV3000_TYPE_CONTROL && c == DV3000_CONTROL_PRODID)
                return System.Text.Encoding.ASCII.GetString(pkt, 5, pkt.Length - 6);
        }

        throw new TimeoutException("Keine ProductId-Antwort vom DV3000-Stick.");
    }

    public async Task<string> GetVersionAsync(int timeoutMs = 3000, CancellationToken ct = default)
    {
        await WriteAsync(DV3000_REQ_VERSTRING, timeoutMs, ct).ConfigureAwait(false);

        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            int remaining = Math.Max(100, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);
            var (t, c, pkt) = await GetResponseAsync(remaining, ct).ConfigureAwait(false);

            if (t == DV3000_TYPE_CONTROL && c == DV3000_CONTROL_VERSTRING)
                return System.Text.Encoding.ASCII.GetString(pkt, 5, pkt.Length - 6);
        }

        throw new TimeoutException("Keine Versions-Antwort vom DV3000-Stick.");
    }

    // ------------------------------------------------------------------
    // Diagnose
    // ------------------------------------------------------------------

    public void Info()
    {
        foreach (var device in _usbManager.DeviceList.Values)
        {
            Log?.Invoke(
                $"USB: {device.DeviceName} " +
                $"VID={device.VendorId:X4} " +
                $"PID={device.ProductId:X4} " +
                $"Manufacturer={device.ManufacturerName} " +
                $"Product={device.ProductName} " +
                $"HasPermission={_usbManager.HasPermission(device)}");
        }
    }
}
