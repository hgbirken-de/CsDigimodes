using DigitalVoice.SoftwareVocoder;
using NLog;

namespace DigitalVoice.AmbeSupport;

/// <summary>
/// Software-Ersatz für den AMBE3000R-Chip: Ein <see cref="IAmbe3000RController"/>, der die DV3000-Pakete der Clients
/// (DMR, YSF, FCS, NXDN, D-STAR) selbst bedient, statt sie an einen Stick oder einen AMBE-Server zu schicken. Die
/// Clients bleiben unverändert.
/// <para>
/// <b>Dekodieren</b> (immer): Channel-Pakete (AMBE-Bits) werden mit dem <see cref="AmbeSoftwareDecoder"/> in Sprachpakete
/// (160 PCM-Samples) umgewandelt.
/// </para>
/// <para>
/// <b>Kodieren</b> (nur wenn ein <see cref="AmbeSoftwareEncoder"/> übergeben wurde): Sprachpakete werden mit diesem Kodierer in
/// Channel-Pakete umgewandelt. Ohne Kodierer beantwortet der Controller Sprachpakete nicht (siehe <see cref="CanEncode"/>):
/// Senden ist dann nicht möglich, die Oberfläche sollte PTT sperren.
/// </para>
/// <para>
/// Ablauf wie beim Chip: <see cref="SendPacket"/> nimmt ein Paket an und legt die Antwort in eine Warteschlange,
/// <see cref="ReceivePacket"/> holt sie ab (<c>null</c>, wenn nichts da ist). Die Clients senden mehrere Pakete hintereinander
/// und lesen danach die Antworten in derselben Reihenfolge (z.B. 3 bei DMR, 5 bei YSF); das funktioniert unverändert. Die
/// Dekodierung geschieht schon in <see cref="SendPacket"/>, weil die Clients dasselbe Paket-Array für das nächste Paket
/// wiederverwenden.
/// </para>
/// <para>Der Controller ist nicht für mehrere Datenströme gleichzeitig gedacht (ein Client, ein Strom).</para>
/// </summary>
public sealed class AmbeSoftwareController : IAmbe3000RController, IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // DV3000-Paketaufbau: 0x61 | Länge (2 Byte, Big Endian, ohne die 4 Kopfbytes) | Typ | Nutzdaten
    private const byte StartByte = 0x61;
    private const byte TypeControl = 0x00;
    private const byte TypeChannel = 0x01;
    private const byte TypeSpeech = 0x02;

    // Steuerpakete (Feld-ID im ersten Nutzdatenbyte)
    private const byte CtrlRateP = 0x0A;        // PKT_RATEP: Rate wählen (legt den Modus fest)
    private const byte CtrlProdId = 0x30;       // PKT_PRODID
    private const byte CtrlVersion = 0x31;      // PKT_VERSTRING
    private const byte CtrlReset = 0x33;        // PKT_RESET
    private const byte CtrlResetSoftCfg = 0x34; // PKT_RESETSOFTCFG
    private const byte CtrlConfigPin = 0x36;    // Abfrage des Konfigurationspins
    private const byte CtrlReady = 0x39;        // PKT_READY

    // Ein Sprachpaket: 6 Kopfbytes + 160 Samples zu 2 Byte (Big Endian)
    private const int SpeechPacketLength = 326;

    // Pause, nach der ein neuer Datenstrom angenommen wird (der Decoder beginnt dann mit leeren Parametern)
    private const long StreamGapMs = 500;

    /// <summary>Der Modus, den die Rate-Wörter des Clients festlegen.</summary>
    private enum Mode
    {
        /// <summary>Noch keine Rate gewählt: Der Modus ergibt sich aus der Bitzahl im Channel-Paket.</summary>
        Unknown,

        /// <summary>DMR: AMBE+2 3600x2450 mit FEC (72 Bit, 9 Byte).</summary>
        Dmr2450x1150,

        /// <summary>YSF, FCS, NXDN: AMBE+2 2450 ohne FEC (49 Bit, 7 Byte).</summary>
        Ambe2450,

        /// <summary>D-STAR: AMBE 3600x2400 mit FEC (72 Bit, 9 Byte).</summary>
        DStar2400x1200,
    }

    private readonly object _lock = new();
    private readonly Queue<byte[]> _responses = new();
    private readonly AmbeSoftwareDecoder _decoder;
    private readonly AmbeSoftwareEncoder? _encoder;
    private readonly short[] _pcm = new short[160];
    private readonly short[] _pcmIn = new short[160];
    private readonly byte[] _ambeOut = new byte[9];

    private Mode _mode = Mode.Unknown;
    private long _lastChannelTick;
    private long _lastSpeechTick;
    private bool _speechWarningLogged;
    private bool _modeWarningLogged;
    private bool _disposed;

    /// <param name="noise">Zufallsquelle für die Rauschanteile (nur für Tests nötig).</param>
    /// <param name="encoder">
    /// Kodierer für das Senden (PCM nach AMBE). Ohne Kodierer kann der Controller nur dekodieren. Der Kodierer braucht eine
    /// Sprachanalyse (<c>ImbeAnalyzer</c> aus dem Projekt <c>DigitalVoice.SoftwareVocoder.Imbe</c>, GPL).
    /// </param>
    public AmbeSoftwareController(NoiseSource? noise = null, AmbeSoftwareEncoder? encoder = null)
    {
        _decoder = new AmbeSoftwareDecoder(noise);
        _encoder = encoder;
    }

    /// <summary>
    /// <c>true</c>, wenn ein Kodierer übergeben wurde und der Controller Sprachpakete (PCM nach AMBE) beantwortet. Sonst kann
    /// nur dekodiert werden, und die Oberfläche sollte die Sendetaste sperren.
    /// </summary>
    public bool CanEncode => _encoder != null;

    // ------------------------------------------------------------------
    // IAmbe3000RController
    // ------------------------------------------------------------------

    /// <inheritdoc />
    public bool IsOpen => !_disposed;

    /// <inheritdoc />
    public void Open()
    {
        lock (_lock)
            ResetState();
    }

    /// <inheritdoc />
    public void Close()
    {
        lock (_lock)
            _responses.Clear();
    }

    /// <inheritdoc />
    public string? GetProductId() => "AMBE-Software";

    /// <inheritdoc />
    public string? GetVersion() => "mbelib 1.3.0 (C# port, decode only)";

    /// <inheritdoc />
    public void Reset()
    {
        lock (_lock)
            ResetState();
    }

    /// <summary>
    /// Dekodiert einen AMBE-Block und liefert 160 PCM-Samples. <paramref name="blockSize"/> ist die Länge des Blocks in Byte
    /// (7 = ohne FEC, 9 = mit FEC).
    /// </summary>
    public short[] Decode(byte[] ambeData, int blockSize)
    {
        ArgumentNullException.ThrowIfNull(ambeData);

        lock (_lock)
        {
            if (_disposed)
                return [];

            var pcm = new short[160];
            bool is49Bit = blockSize <= 7;
            DecodeFrame(ambeData.AsSpan(0, Math.Min(ambeData.Length, blockSize)), is49Bit, pcm);
            return pcm;
        }
    }

    /// <summary>
    /// Kodiert 160 PCM-Samples und liefert den AMBE-Block (7 Byte bei YSF, FCS und NXDN in der Bitreihenfolge des Chips, sonst
    /// 9 Byte). Der Modus ergibt sich aus der zuletzt gewählten Rate.
    /// </summary>
    /// <exception cref="NotSupportedException">Der Controller hat keinen Kodierer (<see cref="CanEncode"/> ist <c>false</c>).</exception>
    public byte[] Encode(short[] pcmSamples)
    {
        ArgumentNullException.ThrowIfNull(pcmSamples);
        if (_encoder == null)
            throw new NotSupportedException("The software vocoder has no encoder.");

        lock (_lock)
        {
            if (_disposed)
                return [];

            int length = EncodeFrame(pcmSamples);
            return length == 0 ? [] : _ambeOut.AsSpan(0, length).ToArray();
        }
    }

    /// <inheritdoc />
    public void SendPacket(byte[] packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        lock (_lock)
        {
            if (_disposed)
                return;   // Der Client wird gerade gestoppt: nichts mehr tun

            if (packet.Length < 5 || packet[0] != StartByte)
            {
                logger.Warn($"Ignoring invalid packet: {Convert.ToHexString(packet)}");
                return;
            }

            switch (packet[3])
            {
                case TypeControl:
                    HandleControl(packet);
                    break;
                case TypeChannel:
                    HandleChannel(packet);
                    break;
                case TypeSpeech:
                    HandleSpeech(packet);
                    break;
                default:
                    logger.Warn($"Ignoring packet of unknown type {packet[3]:X2}.");
                    break;
            }
        }
    }

    /// <inheritdoc />
    public byte[]? ReceivePacket()
    {
        lock (_lock)
            return !_disposed && _responses.Count > 0 ? _responses.Dequeue() : null;
    }

    /// <inheritdoc />
    public byte[]? SendReceivePacket(byte[] packet)
    {
        SendPacket(packet);
        return ReceivePacket();
    }

    /// <inheritdoc />
    public Task<byte[]?> SendReceivePacketAsync(byte[] packet, CancellationToken cancellationToken = default) =>
        Task.FromResult(SendReceivePacket(packet));

    /// <summary>Gibt den Controller frei. Danach tun alle Aufrufe nichts (kein Fehler beim Stoppen eines laufenden Clients).</summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _disposed = true;
            _responses.Clear();
        }
    }

    // ------------------------------------------------------------------
    // Pakete bearbeiten (alle unter _lock)
    // ------------------------------------------------------------------

    private void ResetState()
    {
        _responses.Clear();
        _decoder.Reset();
        _encoder?.Reset();
        _mode = Mode.Unknown;
        _lastChannelTick = 0;
        _lastSpeechTick = 0;
    }

    /// <summary>Steuerpakete: Rate merken, Kennung und Version melden, den Rest wie der Chip mit "OK" quittieren.</summary>
    private void HandleControl(byte[] packet)
    {
        byte id = packet[4];

        switch (id)
        {
            case CtrlRateP:
                SelectRate(packet);
                _responses.Enqueue(Control(id, 0x00));
                break;

            case CtrlProdId:
                _responses.Enqueue(ControlText(id, GetProductId()!));
                break;

            case CtrlVersion:
                _responses.Enqueue(ControlText(id, GetVersion()!));
                break;

            case CtrlReset:
            case CtrlResetSoftCfg:
                _decoder.Reset();
                _encoder?.Reset();
                _lastChannelTick = 0;
                _lastSpeechTick = 0;
                _responses.Enqueue(Control(CtrlReady));   // der Chip meldet sich nach dem Reset mit READY
                break;

            case CtrlConfigPin:
                _responses.Enqueue(Control(id, 0x00));
                break;

            default:
                _responses.Enqueue(Control(id, 0x00));    // übrige Einstellungen: "OK"
                break;
        }
    }

    /// <summary>
    /// Erkennt den Modus an den Rate-Wörtern (<c>PKT_RATEP</c>: 6 Wörter zu je 2 Byte ab Byte 5) wie sie die Clients senden:
    /// DMR <c>0431 0754 2400 ...</c>, YSF/FCS/NXDN <c>0431 0754 0000 ...</c>, D-STAR <c>0130 0763 4000 ...</c>.
    /// </summary>
    private void SelectRate(byte[] packet)
    {
        if (packet.Length < 11)
        {
            logger.Warn("Rate packet is too short.");
            return;
        }

        int rcw0 = (packet[5] << 8) | packet[6];
        int rcw1 = (packet[7] << 8) | packet[8];
        int rcw2 = (packet[9] << 8) | packet[10];

        if (rcw0 == 0x0431 && rcw1 == 0x0754 && rcw2 == 0x2400)
            _mode = Mode.Dmr2450x1150;
        else if (rcw0 == 0x0431 && rcw1 == 0x0754 && rcw2 == 0x0000)
            _mode = Mode.Ambe2450;
        else if (rcw0 == 0x0130 && rcw1 == 0x0763)
            _mode = Mode.DStar2400x1200;
        else
        {
            _mode = Mode.Unknown;
            logger.Warn($"Unknown rate words {rcw0:X4} {rcw1:X4} {rcw2:X4}: the mode follows from the frame size.");
        }

        logger.Debug($"Software vocoder mode: {_mode}");
        _decoder.Reset();
        _encoder?.Reset();
        _lastChannelTick = 0;
        _lastSpeechTick = 0;
    }

    /// <summary>
    /// Channel-Paket <c>61 LLLL 01 01 BB daten...</c> (BB = Bitzahl: 0x48 = 72 Bit, 0x31 = 49 Bit): dekodieren und das
    /// Sprachpaket in die Warteschlange legen.
    /// </summary>
    private void HandleChannel(byte[] packet)
    {
        if (packet.Length < 7 || packet[4] != 0x01)
        {
            logger.Warn($"Ignoring unexpected channel packet: {Convert.ToHexString(packet)}");
            return;
        }

        bool is49Bit = packet[5] <= 0x31;
        int needed = is49Bit ? 7 : 9;
        if (packet.Length - 6 < needed)
        {
            logger.Warn($"Channel packet too short ({packet.Length - 6} of {needed} data bytes).");
            return;
        }

        // Nach einer Pause beginnt ein neuer Datenstrom: Der Decoder startet mit leeren Parametern
        long now = Environment.TickCount64;
        if (_lastChannelTick != 0 && now - _lastChannelTick > StreamGapMs)
            _decoder.Reset();
        _lastChannelTick = now;

        DecodeFrame(packet.AsSpan(6, needed), is49Bit, _pcm);

        // Sprachpaket wie vom Chip: 61 0142 02 00 A0 + 160 Samples, Big Endian
        var response = new byte[SpeechPacketLength];
        response[0] = StartByte;
        response[1] = 0x01;
        response[2] = 0x42;
        response[3] = TypeSpeech;
        response[4] = 0x00;
        response[5] = 0xA0;
        for (int i = 0; i < 160; i++)
        {
            response[6 + (2 * i)] = (byte)((_pcm[i] >> 8) & 0xFF);
            response[7 + (2 * i)] = (byte)(_pcm[i] & 0xFF);
        }

        _responses.Enqueue(response);
    }

    /// <summary>
    /// Wählt die Decoder-Variante nach Modus und Bitzahl. Frames mit 49 Bit (YSF, FCS, NXDN) kommen in der Bitreihenfolge des
    /// DVSI-Chips an, Frames mit 72 Bit (DMR, D-STAR) unverändert.
    /// </summary>
    private void DecodeFrame(ReadOnlySpan<byte> ambe, bool is49Bit, Span<short> pcm)
    {
        if (is49Bit)
        {
            // Die Clients ordnen die 49 Bits für den DVSI-Chip um (NxdnCodec.Interleave, Fusion-Codec): hier zurückordnen
            _decoder.Decode2450DvsiOrder(ambe, pcm);
            return;
        }

        if (_mode == Mode.DStar2400x1200)
            _decoder.Decode2400x1200(ambe, pcm);
        else
            _decoder.Decode2450x1150(ambe, pcm);   // DMR, oder 72 Bit ohne bekannte Rate
    }

    /// <summary>
    /// Sprachpaket <c>61 0142 02 00 A0 + 160 Samples</c> (Big Endian) zum Kodieren: Mit Kodierer wird ein Channel-Paket als
    /// Antwort bereitgestellt (<c>61 00 0B 01 01 48 + 9 Byte</c> bei 72 Bit, <c>61 00 09 01 01 31 + 7 Byte</c> bei 49 Bit).
    /// Ohne Kodierer gibt es keine Antwort (der Client erhält <c>null</c>).
    /// </summary>
    private void HandleSpeech(byte[] packet)
    {
        if (_encoder == null)
        {
            if (!_speechWarningLogged)
            {
                _speechWarningLogged = true;
                logger.Warn("The software vocoder has no encoder: speech packets are ignored (transmitting is not possible).");
            }

            return;
        }

        if (packet.Length < SpeechPacketLength || packet[4] != 0x00 || packet[5] != 0xA0)
        {
            logger.Warn($"Ignoring unexpected speech packet of {packet.Length} bytes.");
            return;
        }

        // Nach einer Pause beginnt ein neuer Datenstrom (neue Sendung): Der Kodierer startet mit leerem Gedächtnis
        long now = Environment.TickCount64;
        if (_lastSpeechTick != 0 && now - _lastSpeechTick > StreamGapMs)
            _encoder.Reset();
        _lastSpeechTick = now;

        for (int i = 0; i < 160; i++)
            _pcmIn[i] = (short)((packet[6 + (2 * i)] << 8) | packet[7 + (2 * i)]);

        int length = EncodeFrame(_pcmIn);
        if (length == 0)
            return;

        var response = new byte[6 + length];
        response[0] = StartByte;
        response[1] = 0x00;
        response[2] = (byte)(2 + length);
        response[3] = TypeChannel;
        response[4] = 0x01;
        response[5] = length == 7 ? (byte)0x31 : (byte)0x48;   // 49 oder 72 Bit
        Array.Copy(_ambeOut, 0, response, 6, length);
        _responses.Enqueue(response);
    }

    /// <summary>
    /// Kodiert einen Frame nach dem gewählten Modus in <see cref="_ambeOut"/> und liefert die Länge in Byte (0 = nicht möglich).
    /// Bei 49 Bit steht die Bitreihenfolge des DVSI-Chips im Ergebnis.
    /// </summary>
    private int EncodeFrame(ReadOnlySpan<short> pcm)
    {
        switch (_mode)
        {
            case Mode.Dmr2450x1150:
                _encoder!.Encode2450x1150(pcm, _ambeOut);
                return 9;
            case Mode.Ambe2450:
                _encoder!.Encode2450DvsiOrder(pcm, _ambeOut);
                return 7;
            case Mode.DStar2400x1200:
                _encoder!.Encode2400x1200(pcm, _ambeOut);
                return 9;
            default:
                if (!_modeWarningLogged)
                {
                    _modeWarningLogged = true;
                    logger.Warn("No rate selected yet: the mode for encoding is unknown.");
                }

                return 0;
        }
    }

    // ------------------------------------------------------------------
    // Antwortpakete bauen
    // ------------------------------------------------------------------

    /// <summary>Steuerpaket-Antwort <c>61 00 LL 00 id [daten]</c>.</summary>
    private static byte[] Control(byte id, params byte[] data)
    {
        var packet = new byte[5 + data.Length];
        packet[0] = StartByte;
        packet[1] = 0x00;
        packet[2] = (byte)(1 + data.Length);
        packet[3] = TypeControl;
        packet[4] = id;
        data.CopyTo(packet, 5);
        return packet;
    }

    /// <summary>Antwort mit einer Zeichenkette (Product-ID, Version): <c>61 00 LL 00 id text 00</c>.</summary>
    private static byte[] ControlText(byte id, string text)
    {
        byte[] ascii = System.Text.Encoding.ASCII.GetBytes(text);
        var data = new byte[ascii.Length + 1];   // Text und abschließendes 0x00
        ascii.CopyTo(data, 0);
        return Control(id, data);
    }
}
