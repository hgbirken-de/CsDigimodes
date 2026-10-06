using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using DigitalVoice.DStar.Common;
using NLog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace DigitalVoice.DStar.Ref;

/// <summary>
/// Implementation of a Digital Call Server (REF) client.
/// </summary>
public class RefClient
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    readonly object _udpLock = new();

    readonly RefClientConfig _cfg;

    readonly DStarSessionContext _clientState;

    readonly int _socketTimeout = 1000; // ms

    UdpClient? _udpClient;

    Thread? _udpPacketReaderThread; // read REF UDP packets

    readonly System.Timers.Timer _pingTimer = new(2000);

    readonly System.Timers.Timer _rxTimer = new(100);

    // ---- TX: Mikrofon -> PCM-Queue (gehoert dem MicrophoneReader2) -> EIN Worker-Thread -> AMBE-Chip -> UDP ----
    const int PcmBlockBytes = 320;                    // 20 ms PCM: 160 Samples * 2 Bytes

    readonly object _chipLock = new();                // jede Transaktion mit dem AMBE-Chip exklusiv (TX-Worker und RxTimerCallback)
    readonly object _frameLock = new();               // Frame-Versand des Workers gegen das Sendeende abgrenzen
    readonly AutoResetEvent _micSignal = new(false);  // vom Mikrofon geweckt (AudioAvailable)
    
    CancellationTokenSource? _txCts;
    Thread? _txThread;

    readonly ConcurrentQueue<byte[]> _rxQueue = new();

    PacketRecorder? _packetRecorder; // used for read/write operations, only one at a time

    int _rxInactivityTicks = 0;

    // REF data Consumer delegate
    public delegate void ConsumeRefData(DStarSessionContext state);
    public delegate void ConsumeNetMsg(string msg);

    public ConsumeRefData? ExternalRefDataConsumer;
    public ConsumeNetMsg? ExternalNetMsgConsumer;


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cfg"></param>
    public RefClient(RefClientConfig cfg)
    {
        cfg.Validate();
        _cfg = cfg;

        _clientState = new();

        _pingTimer.Elapsed += PingTimerCallback;
        _rxTimer.Elapsed += RxTimerCallback;

        if (_cfg.SimulationMode)
        {
            //throw new NotImplementedException("Simulation mode not implemented yet.");
        }
        else
        {
            Connect();
        }
    }

    /// <summary>
    /// Initializes the DV3000 device by sending a predefined sequence of control packets,
    /// including configuration query, software reset, encoder mode setup, and custom rate selection.
    /// </summary>
    private void InitDV3000()
    {
        byte[][] packets =
        [
            [0x61, 0x00, 0x01, 0x00, 0x36], // Query for configuration pin state at power-up or reset
            [0x61, 0x00, 0x07, 0x00, 0x34, 0x05, 0x00, 0x00, 0x07, 0x00, 0x10], // Reset the device with software configuration
            [0x61, 0x00, 0x03, 0x00, 0x05, 0x10, 0x40], // Encoder cmode flags for current channel
            [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x01, 0x30, 0x07, 0x63, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48] // custome rate, interoperable with D-Star as stated by DVSI
        ];

        foreach (var p in packets)
        {
            byte[]? resp = _cfg.AmbeController!.SendReceivePacket(p);
            logger.Debug($"send: {Convert.ToHexString(p)}");
            logger.Debug($"rcvd: {Convert.ToHexString(resp)}");
        }
    }

    /// <summary>
    /// Establishes a UDP connection to the configured REF reflector address and port.
    /// </summary>
    /// <remarks>
    /// Initializes the <see cref="_udpClient"/> with the server endpoint and sets a receive timeout.
    /// Logs the local endpoint details upon successful connection. If the connection fails,
    /// logs the error and throws an exception.
    /// </remarks>
    /// <exception cref="Exception">
    /// Thrown if the local endpoint could not be determined or if a <see cref="SocketException"/> occurs during connection.
    /// </exception>
    private void Connect()
    {
        try
        {
            // Resolve hostname synchronously
            IPAddress[] addresses = Dns.GetHostAddresses(_cfg.RefAddress);
            if (addresses.Length == 0)
            {
                logger.Error($"No IP addresses found for hostname '{_cfg.RefAddress}'");
                return;
            }
            //IPAddress targetAddress = addresses[0];

            _udpClient = new UdpClient(_cfg.RefAddress, _cfg.RefPort);
            _udpClient.Client.ReceiveTimeout = _socketTimeout;
            if (_udpClient.Client.LocalEndPoint is not IPEndPoint localEp)
            {
                throw new Exception($"Unable to created endpoint, {_cfg.RefAddress}:{_cfg.RefPort}");
            }
            logger.Debug($"Local socket bound to {localEp.Address}:{localEp.Port}, timeout={_socketTimeout} ms");
        }
        catch (SocketException ex)
        {
            logger.Error($"{ex.Message}, ErrorCode = {ex.ErrorCode}");
            _udpClient?.Dispose();
            throw new Exception($"Unable to connect to BM server at {_cfg.RefAddress}:{_cfg.RefPort}", ex);
        }
    }

    public static string FormatCallsign(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new string(' ', 8);

        // Trim and split on whitespace
        var parts = input.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length > 1)
        {
            // First part padded to 7 chars, then append second part
            var first = parts[0].Trim();
            if (first.Length < 7)
                first = first.PadRight(7, ' ');

            var second = parts[1].Trim();
            return first + second;
        }
        else
        {
            // Single part, padded to 8 chars
            var s = parts[0].Trim();
            return s.PadRight(8, ' ');
        }
    }

    /// <summary>
    /// Call back method to login the FCS reflector (keep alive). 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PingTimerCallback(object? sender, ElapsedEventArgs e)
    {
        SendPing();
    }

    private void ProcessRcvdUdpPacket(byte[] packet)
    {
        logger.Debug($"({packet.Length}) - {Convert.ToHexString(packet)}");

        switch (packet.Length)
        {
            case 3 when packet[0] == 3:
                {
                    // keep alive ping
                    _clientState.RxPingCnt++;
                    if (_clientState.RxStreamState is StreamState.Lost or StreamState.End)
                    {
                        _clientState.RxStreamState = StreamState.Idle;
                    }
                }
                return;
            case 5 when packet[0] == 5:
                SendLogin();
                return;
            case 8 when packet[0] == 8:
                {
                    string resp = Encoding.ASCII.GetString(packet, 4, 4);
                    logger.Debug($"Connected: {resp}");
                    if (resp == "OKRW" || resp == "OKRO" || resp == "BUSY")
                    {
                        _pingTimer.Start();
                    }
                    else if (resp == "FAIL")
                    {
                        // TODO: update disconnect
                    }
                    else
                    {
                        logger.Debug($"Unknown response: {resp}");
                    }
                }
                return;
            case 29 when packet[0] == 29 && packet[1] == 0x80:
                if (Encoding.ASCII.GetString(packet, 2, 4) == "DSVT")
                {
                    // TODO: -> RefCodec.Decode(...)
                    ushort streamId = (ushort)(packet[14] << 8 | packet[15] & 0xFF);
                    if (streamId != _clientState.RxStreamId)
                        return;

                    _rxInactivityTicks = 0;

                    byte[] ambeData = RefCodec.DecodeVoiceFrame(packet, _clientState);
                    _rxQueue.Enqueue(ambeData);
                    ExternalRefDataConsumer?.Invoke(_clientState);
                }
                return;
            case 32 when packet[0] == 32 && packet[1] == 0x80: // end of RX stream
                if (Encoding.ASCII.GetString(packet, 2, 4) == "DSVT")
                {
                    ushort streamId = (ushort)(packet[14] << 8 | packet[15] & 0xFF);
                    if (streamId == _clientState.RxStreamId) {
                        _clientState.RxStreamId = 0;
                        _clientState.RxStreamState = StreamState.End;
                        _rxInactivityTicks = 0;
                    }
                    ExternalRefDataConsumer?.Invoke(_clientState);
                }
                return;
            case 58 when packet[0] == 58 && packet[1] == 0x80:
                if (Encoding.ASCII.GetString(packet, 2, 4) == "DSVT")
                {
                    // Decode the whole 32-byte block once
                    string headerBlock = Encoding.ASCII.GetString(packet, 20, 32);

                    // Extract fields (8 bytes each), trimming trailing spaces
                    _clientState.RxRptr2 = headerBlock[..8].TrimEnd();
                    _clientState.RxRptr1 = headerBlock.Substring(8, 8).TrimEnd();
                    _clientState.RxUrCall = headerBlock.Substring(16, 8).TrimEnd();
                    _clientState.RxSrc = headerBlock.Substring(24, 8).TrimEnd();

                    string refCallsign =  RefCallsign(_cfg.RefName, _cfg.Module);

                    logger.Debug($"(a) {_clientState.RxRptr2} {_clientState.RxRptr1} {_clientState.RxUrCall} {_clientState.RxSrc} {refCallsign}");
                    if (_clientState.RxRptr2 == refCallsign || _clientState.RxRptr1 == refCallsign)
                    {
                        _clientState.RxStreamId = (ushort)(packet[14] << 8 | packet[15] & 0xFF);
                        _clientState.RxStreamState = StreamState.New;
                        if (!_rxTimer.Enabled)
                        {
                            _rxTimer.Start();
                        }
                        logger.Debug($"(b) in if-block, _rxTimer.Enabled: {_rxTimer.Enabled}");
                        // TOOD:
                        // sd_gps_cnt = 0;
                        // clear GPS data
                    }
                }
                return;
            default:
                logger.Error($"Unexpected packet: len {packet.Length} - {Convert.ToHexString(packet)}");
                return;
        }
    }

    private static string RefCallsign(string refName, char module)
    {
        if (refName.Length > 7)
        {
            throw new ArgumentException($"Lengths of {nameof(refName)} exceeds 7 chars!");
        }
        return refName.PadRight(7) + module;
    }


    /// <summary>
    /// Continuously reads REF UDP packets from the network and dispatches them for processing.
    /// </summary>
    /// <remarks>
    /// This method runs in a loop while the <c>_isRunning</c> flag is <c>true</c>. It receives UDP datagrams from the configured 
    /// UDP client, logs the received data in hexadecimal format, and forwards each packet to <see cref="ProcessRcvdUdpPacket"/> 
    /// for protocol handling.
    ///
    /// If <c>_simulationMode</c> is enabled, the method throws a <see cref="NotImplementedException"/> since simulation is not yet implemented.
    ///
    /// All exceptions are caught and logged. Upon exit, it logs the total number of packets received and attempted to process.
    /// </remarks>
    private void ReadUdpPackets()
    {
        int packetCount = 0;
        try
        {
            while (_clientState.IsRunning)
            {
                try
                {
                    byte[]? packet = null;
                    if (_cfg.SimulationMode)
                    {
                        packet = _packetRecorder?.ReadNextPacket();
                        if (packet == null)
                            break;
                    }
                    else
                    {
                        IPEndPoint remoteEp = new(IPAddress.Any, _cfg.RefPort);
                        packet = _udpClient!.Receive(ref remoteEp);
                        if (_cfg.RecordRefPackets)
                            _packetRecorder?.WritePacket(packet);
                    }

                    packetCount++;
                    ProcessRcvdUdpPacket(packet);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut) {}
                catch (OperationCanceledException) when (!_clientState.IsRunning)
                {
                    // Erwarteter Abbruch: Stop() hat den seriellen Port geschlossen, während dieser
                    // Thread gerade blockierend in SerialPort.ReadByte() hing. Kein echter Fehler,
                    // sondern normales, absichtliches Herunterfahren - deshalb nur Debug statt ERROR.
                    logger.Debug("Read cancelled due to shutdown (Stop() was called).");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex);
        }
        finally
        {
            logger.Debug($"Packets read: {packetCount}");
            _udpClient?.Dispose();
            _udpClient = null;

            _packetRecorder?.Close();
            _packetRecorder = null;

            _cfg.WavPcmRecorder?.Close();
        }
    }

    readonly byte[] ambeChannelPacket = [0x61, 0x00, 0x0B, 0x01, 0x01, 0x48, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]; // 15 bytes 

    /// <summary>
    /// Processes incoming speech packets on a timer interval (20ms).
    /// 
    /// This method is called periodically by a timer to handle received audio data:
    /// - Increments the RX watchdog counter to detect inactivity.
    /// - If inactivity exceeds threshold (100 ticks), marks the stream status as lost.
    /// - Attempts to read an audio packet from the rx queue and feeds it to the audio player.
    /// - If no packet is available and the stream is lost or ended, stops the timer and sets status to idle.
    /// </summary>
    /// <param name="sender">The timer object invoking this method (can be null).</param>
    /// <param name="e">Elapsed event arguments associated with the timer event.</param>
    private void RxTimerCallback(object? sender, ElapsedEventArgs e)
    {
        //logger.Debug("");
        //Stopwatch sw = Stopwatch.StartNew();

        if (++_rxInactivityTicks > 100)
        {
            _rxInactivityTicks = 0;
            _clientState.RxStreamId = 0;
            _clientState.RxStreamState = StreamState.Lost;
        }

        const int n = 5; // <-- this must match the timer period (5 -> 100ms) 
        if (_clientState.TransceiveMode is TransceiveMode.Rx && _rxQueue.Count >= n)
        {
            // Die ganze Folge "5 senden, 5 lesen" exklusiv: der TX-Worker darf dazwischen nicht an den Chip.
            lock (_chipLock)
            {
                for (int i = 0; i < n; i++)
                {
                    if (_rxQueue.TryDequeue(out byte[]? ambeData))
                    {
                        if (ambeData.Length != 9)
                            throw new ArgumentException($"Invalid AMBE data len: {Convert.ToHexString(ambeData)}");

                        ambeData.CopyTo(ambeChannelPacket, 6);
                        _cfg.AmbeController!.SendPacket(ambeChannelPacket);
                    }
                    else
                    {
                        logger.Error("Unable to read rx queue");
                    }
                }

                for (int i = 0; i < n; i++)
                {
                    byte[]? pcmData = _cfg.AmbeController!.ReceivePacket();
                    if (AmbeHelper.IsSpeechPacket(pcmData))
                    {
                        AmbeHelper.SwapPcmBytes(pcmData!);
                        _cfg.AudioPlayer?.FeedPcmData(pcmData!, 6, pcmData!.Length - 6);
                        _cfg.WavPcmRecorder?.WritePcm(pcmData!, 6, pcmData!.Length - 6);
                    }
                    else
                    {
                        logger.Error("Unexpected response from AMBE server: {0}", pcmData != null ? Convert.ToHexString(pcmData) : "null");
                    }
                }
            }
        }
        else if (_clientState.RxStreamState is StreamState.Lost or StreamState.End)
        {
            _rxInactivityTicks = 0;
            _rxTimer.Stop();
            _clientState.RxStreamId = 0;
            _clientState.RxStreamState = StreamState.Idle;
            //logger.Debug("Channel empty");
        }
        ExternalRefDataConsumer?.Invoke(_clientState);
        //sw.Stop();
        //logger.Debug($"{sw.ElapsedMilliseconds}");
    }

    /// <summary>
    /// Send a connect message to the reflector.
    /// </summary>
    public void SendConnect()
    {
        logger.Debug($"Callsign= {_cfg.Callsign} , Reflector= {_cfg.RefName} , Module= {_cfg.Module}");
        byte[] msg = [0x05, 0x00, 0x18, 0x00, 0x01];
        SendDatagram(msg, msg.Length);
    }

    public void SendLogin()
    {
        logger.Debug("");
        var rnd = new Random();
        int x = rnd.Next(7245, 999999);
        string serial = "HS" + x.ToString("D6"); // TODO: VS instead of HS

        string callsign = _cfg.Callsign.PadRight(6, ' ');

        // total length = 4 + 6 + 10 + 8 = 28
        byte[] outBuf = new byte[28];

        // header
        outBuf[0] = 0x1c;
        outBuf[1] = 0xc0;
        outBuf[2] = 0x04;
        outBuf[3] = 0x00;

        // callsign (6 bytes, ASCII, padded with spaces)
        Encoding.ASCII.GetBytes(callsign, 0, 6, outBuf, 4);

        // 10 bytes padding (already zeroed)

        // serial (ASCII, "HSxxxxxx", 8 bytes)
        Encoding.ASCII.GetBytes(serial, 0, serial.Length, outBuf, 20);

        SendDatagram(outBuf, outBuf.Length);
    }

    /// <summary>
    /// Send the argument bytes to the reflector. 
    /// </summary>
    /// <param name="dgram">The bytes to send.</param>
    /// <param name="len">The length of the datagram to sent.</param>
    private void SendDatagram(byte[] dgram, int len)
    {
        lock (_udpLock)
        {   // UdpClient is not thread safe
            _udpClient?.Send(dgram, len);
        }
    }

    /// <summary>
    /// Send a disconnect message to the reflector.
    /// </summary>
    public void SendDisconnect()
    {
        logger.Debug("");
        byte[] msg = [0x05, 0x00, 0x18, 0x00, 0x00];
        SendDatagram(msg, msg.Length);
    }

    /// <summary>
    /// Send a ping message to the reflector.
    /// </summary>
    public void SendPing()
    {
        logger.Debug("");
        byte[] msg = [0x03, 0x60, 0x00];
        SendDatagram(msg, msg.Length);
    }

    /// <summary>
    /// Starts this this REF client.
    /// </summary>
    public void Start()
    {
        logger.Debug($"{_clientState.IsRunning}");
        if (_clientState.IsRunning)
        {
            logger.Error($"Client is already running.");
            return;
        }

        _cfg.AmbeController!.Open();

        InitDV3000();

        if (_cfg.SimulationMode && _cfg.RecordRefPackets)
        {
            _cfg.RecordRefPackets = false; // never record test data
            logger.Warn($"Set {nameof(_cfg.RecordRefPackets)}={_cfg.RecordRefPackets}, reason: simulation mode is active.");
        }

        string dataDir = "data";
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        // Setup packet recorder if need
        if (_cfg.RecordRefPackets)
        {
            string packetFile = Path.Combine("data", $"ref_udp_packets_{DateTime.Now:yyyyMMddHHmmss}.bin");
            _packetRecorder = new PacketRecorder(packetFile, FileMode.Create, FileAccess.Write);
        }

        // Setup for simulation mode if needed
        if (_cfg.SimulationMode)
        {
            ArgumentException.ThrowIfNullOrEmpty(_cfg.SimulationFile, nameof(_cfg.SimulationFile));
            _packetRecorder = new PacketRecorder(_cfg.SimulationFile, FileMode.Open, FileAccess.Read);
            logger.Debug($"Simulation mode, input file: {_cfg.SimulationFile}");
        }

        _clientState.RxPingCnt = 0;
        _clientState.IsRunning = true;
        _udpPacketReaderThread = new Thread(ReadUdpPackets) { IsBackground = true };
        _udpPacketReaderThread.Start();
        SendConnect();
    }

    /// <summary>
    /// Stops this REF client.
    /// </summary>
    public void Stop()
    {
        logger.Debug($"{_clientState.IsRunning}");
        _clientState.IsRunning = false;
        _pingTimer.Stop();
        SendDisconnect(); // TODO: check cmd sequence

        StopTxResources(); // falls gerade gesendet wird: Mikrofon und TX-Worker beenden, bevor der Chip geschlossen wird

        _cfg.AmbeController!.Close();
    }

    /// <summary>
    /// Starts or stops a transmission.
    /// </summary>
    /// <param name="arg">Controls start/stop: true -> start transmission, false -> stop transmission</param>
    public void StartStopTransmit(bool arg)
    {
        logger.Debug($"arg={arg}");
        if (arg)
        {
            if (!_clientState.IsRunning)
                throw new InvalidOperationException("The client must be started before transmitting.");

            // Queue des Mikrofon-Readers VOR jeder Zustandsaenderung holen: wirft der Reader hier (z.B. ein
            // Reader ohne Queue), ist noch nichts veraendert und es wurde nichts gesendet.
            ConcurrentQueue<byte[]>? pcmQueue = _cfg.MicrophoneReader?.GetPcmQueue();

            _clientState.TransceiveMode = TransceiveMode.Tx;
            _clientState.RxStreamState = StreamState.Transmitting;
            
            _rxTimer.Stop();
            _rxQueue.Clear();
            // TODO: make this sane!
            _clientState.TxRptr1 = _cfg.Callsign.PadRight(7, ' ') + 'C';
            _clientState.TxRptr2 = _cfg.RefName.PadRight(7, ' ') + _cfg.Module;
            _clientState.TxMyCall = _cfg.Callsign;
            _clientState.TxUrCall = "CQCQCQ";

            _clientState.TxUsrMsg = "CsDigimodes".PadRight(20, ' ');

            _clientState.TxFrameCnt = 0;
           
            if (pcmQueue != null && _cfg.MicrophoneReader is { } mic)
            {
                StartTxWorker();                          // zuerst der Verbraucher ...
                mic.AudioAvailable += OnMicSignal;        // ... dann der Wecker ...
                mic.GetPcmQueue().Clear();
                mic.Start();                              // ... dann das Mikrofon
            }
        }
        else
        {
            StopTxResources(); // Mikrofon stoppen, Worker beenden, Queue leeren

            lock (_frameLock) // nach dem Worker-Stopp kann kein Frame mehr nach dem Sendeende rausgehen
            {
                _clientState.TransceiveMode = TransceiveMode.Rx;

                // Aufraeumen nach der Sendung (frueher im else-Zweig des TxTimerCallback)
                //byte[] lastFrame = RefCodec.Create(_clientState, new byte[9]);
                //SendDatagram(lastFrame, lastFrame.Length);
                _clientState.TxFrameCnt = 0;
                _clientState.TxStreamId = 0;
            }
        }
    }

    /// <summary>Wecker fuer den TX-Worker. Laeuft auf dem Mikrofon-Aufnahme-Thread: nur signalisieren, nichts verarbeiten.</summary>
    private void OnMicSignal(object? sender, byte[] block) => _micSignal.Set();

    /// <summary>Startet den TX-Worker (einziger Thread, der im Sendebetrieb den Chip fuer die Kodierung anspricht).</summary>
    private void StartTxWorker()
    {
        StopTxWorker();                  // falls noch einer laeuft
        _cfg.MicrophoneReader!.GetPcmQueue().Clear();  // Reste einer frueheren Sendung verwerfen
        _micSignal.Reset();

        _txCts = new CancellationTokenSource();
        CancellationToken ct = _txCts.Token;
        _txThread = new Thread(() => TxWorker(ct)) { IsBackground = true, Name = "RefTx" };
        _txThread.Start();
    }

    /// <summary>Beendet den TX-Worker. Nach der Rueckkehr setzt er keine Frames mehr ab.</summary>
    private void StopTxWorker()
    {
        CancellationTokenSource? cts = _txCts;
        Thread? thread = _txThread;
        _txCts = null;
        _txThread = null;
        if (cts == null)
            return;

        cts.Cancel();
        _micSignal.Set(); // Worker aus WaitOne wecken

        if (thread != null && thread != Thread.CurrentThread)
        {
            if (thread.Join(500))
                cts.Dispose();
            else
                logger.Warn("TX worker did not stop within 500 ms (AMBE chip timeout?).");
        }
    }

    /// <summary>Mikrofon abmelden/stoppen, Worker beenden, PCM-Queue leeren (idempotent).</summary>
    private void StopTxResources()
    {
        try
        {
            if (_cfg.MicrophoneReader is { } mic)
            {
                mic.AudioAvailable -= OnMicSignal;
                mic.Stop();
                mic.GetPcmQueue().Clear();
            }
        }
        finally
        {
            StopTxWorker();
        }
    }

    /// <summary>
    /// TX-Worker: holt die vom Mikrofon eingereihten 20-ms-PCM-Bloecke aus der Queue, kodiert sie einzeln am
    /// AMBE-Chip und sendet jeden AMBE-Block (9 Bytes) als eigenen REF-Frame (D-STAR: 1 Block = 1 Frame = 20 ms).
    /// Nur dieser Thread spricht im Sendebetrieb den Chip an (zusammen mit RxTimerCallback() ueber _chipLock abgesichert).
    /// </summary>
    private void TxWorker(CancellationToken ct)
    {
        // Sprachpaket fuer den Chip: Header + 160 PCM-Samples (nur dieser Thread benutzt das Array)
        byte[] speechPacket = new byte[6 + PcmBlockBytes];
        speechPacket[0] = 0x61;
        speechPacket[1] = 0x01; // len field 320 + 6 - 4 = 322 -> 0x0142
        speechPacket[2] = 0x42;
        speechPacket[3] = 0x02;
        speechPacket[4] = 0x00;
        speechPacket[5] = 0xA0; // 160 PCM samples

        ConcurrentQueue<byte[]> queue = _cfg.MicrophoneReader!.GetPcmQueue();

        long sent = 0; // nur zur Diagnose (RefCodec.Create verwaltet TxFrameCnt selbst)

        try
        {
            while (!ct.IsCancellationRequested)
            {
                _micSignal.WaitOne(100); // vom Mikrofon geweckt; Timeout = Sicherheitsnetz

                while (!ct.IsCancellationRequested && queue.TryDequeue(out byte[]? pcm))
                {
                    if (pcm.Length != PcmBlockBytes)
                    {
                        logger.Error($"Unexpected PCM block size: {pcm.Length} (expected {PcmBlockBytes})");
                        continue;
                    }

                    pcm.CopyTo(speechPacket, 6);
                    AmbeHelper.SwapPcmBytes(speechPacket); // LE -> BE

                    byte[]? ambe;
                    lock (_chipLock)
                    {
                        _cfg.AmbeController!.SendPacket(speechPacket);
                        ambe = _cfg.AmbeController.ReceivePacket();
                    }

                    if (!AmbeHelper.IsAmbePacket(ambe))
                    {
                        logger.Error($"Unexpected response of AMBE server: {(ambe != null ? Convert.ToHexString(ambe) : "null")}");
                        continue;
                    }

                    byte[] ambeData = ambe![6..]; // ignore 6 byte header
                    if (ambeData.Length != 9)
                    {
                        logger.Error($"Unexpected AMBE block length: {ambeData.Length} (expected 9)");
                        continue;
                    }

                    lock (_frameLock)
                    {
                        if (ct.IsCancellationRequested)
                            break; // Sendung wurde beendet: keinen Frame mehr nach dem Sendeende absetzen

                        byte[] refFrame = RefCodec.Create(_clientState, ambeData);
                        SendDatagram(refFrame, refFrame.Length);
                        _clientState.RxStreamState = StreamState.Transmitting;
                    }

                    sent++;
                    if (sent % 10 == 0) // 50 Frames/s: nicht jedes einzeln loggen
                        logger.Debug($"Sent voice frame: {sent}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception in TxWorker.");
        }
    }
}