using AmbeServer;
using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using FusionCodec;
using NLog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace DigitalVoice.Fusion;

/// <summary>
/// YSF client implementation.
/// https://ycs-wiki.xreflector.net/doku.php?id=start:protocols:ysf
/// </summary>
public sealed class YsfClient
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte[] ambeChannelPacket = [0x61, 0x00, 0x09, 0x01, 0x01, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]; // len field: 0x0009 (7 + 2 bytes)

    static readonly byte[] ambeSpeechPacket = new byte[326];

    readonly object _lock = new();

    readonly YsfClientConfig _cfg;

    UdpClient? _udpClient;
    readonly int _socketTimeout = 2000;
    bool _isRunning = false;

    PacketRecorder? _packetReader;
    PacketRecorder? _packetRecorder;

    WavPcmRecorder? _wavRecorder;

    readonly ConcurrentQueue<byte[]> _rxQueue = new();

    readonly ConcurrentQueue<byte[]> _txQueue = new();

    readonly AmbeClient _ambeClient;

    readonly System.Timers.Timer _pingTimer = new(5000);

    readonly System.Timers.Timer _rxTimer = new(20); // corresponds to 1 AMBE/PCM block

    readonly System.Timers.Timer _txTimer = new(20);

    Thread? _udpReaderThread; // read YSF UDP packets

    readonly YsfSessionContext _sessionCtx = new(true);

    int _rxInactivityTicks = 0;

    // YSF data Consumer delegate
    public delegate void ConsumeYsfData(YsfSessionContext ysfClientState);

    public ConsumeYsfData? ExternalYsfDataConsumer;

    static YsfClient()
    {
        // init AMBE specch packet
        ambeSpeechPacket[0] = 0x61;
        ambeSpeechPacket[1] = 0x01; // len field 320 + 6 - 4 = 322 -> 0x0142
        ambeSpeechPacket[2] = 0x42;
        ambeSpeechPacket[3] = 0x02;
        ambeSpeechPacket[4] = 0x00;
        ambeSpeechPacket[5] = 0xA0; // 160 PCM samples
    }


    /// <summary>
    /// Constructor that creates a new YsfClient instance.
    /// </summary>
    /// <param name="cfg">Configuration parameters for this YSF client instance.</param>
    public YsfClient(YsfClientConfig cfg)
    {
        _cfg = cfg;
        _cfg.Validate();

        _pingTimer.Elapsed += PingTimerCallback;
        _rxTimer.Elapsed += RxTimerCallback;
        _txTimer.Elapsed += TxTimerCallback;

        _ambeClient = new(_cfg.AmbeSrvAddr, _cfg.AmbeSrvPort);

        InitDV3000();
    }


    /// <summary>
    /// Sends a list of AMBE-encoded blocks to the AMBE server for decoding (i.e. decompressing), and writes them to the output channel.
    /// Any invalid or unexpected responses are logged.
    /// </summary>
    private void DecompressAmbe()
    {
        if (_sessionCtx.AmbeData.Count == 0)
            return;

        foreach (var b in _sessionCtx.AmbeData)
        {
            b.CopyTo(ambeChannelPacket, 6);
            _ambeClient?.SendPacket(ambeChannelPacket);
        }

        for (int i = 0; i < _sessionCtx.AmbeData.Count; i++)
        {
            byte[]? resp = _ambeClient?.ReceivePacket();
            if (AmbeHelper.IsSpeechPacket(resp))
            {
                _rxQueue.Enqueue(resp!);
            }
            else
            {
                logger.Error("Unexpected response from AMBE server: ", resp != null ? Convert.ToHexString(resp) : "null");
            }
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
            //[0x61, 0x00, 0x0d, 0x00, 0x0a, 0x01, 0x30, 0x07, 0x63, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48], // Select custom rate for current channel
            [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x04, 0x31, 0x07, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x31], // *** Select custom rate for current channel
            //[0x61, 0x00, 0x0d, 0x00, 0x0a, 0x04, 0x31, 0x07, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x39, 0x36], // Select custom rate for current channel
        ];

        foreach (var p in packets)
        {
            byte[]? resp = _ambeClient!.SendReceivePacket(p);
            logger.Debug("{}", resp != null ? Convert.ToHexString(resp) : "null");
        }
    }


    /// <summary>
    /// Call back method to login/ping the YSF reflector. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PingTimerCallback(object? sender, ElapsedEventArgs e)
    {
        SendYsfp();
    }


    /// <summary>
    /// Process a received YSF packet.
    /// </summary>
    /// <param name="packet">The full YSF packet.</param>
    public void ProcessRcvdPacket(byte[] packet)
    {
        //logger.Debug(Convert.ToHexString(packet));
        string signature = Encoding.ASCII.GetString(packet, 0, 4);
        switch (signature)
        {
            case "YSFD" when (packet.Length == 155):
                // -> payload processing
                break;
            case "YSFP" when (packet.Length == 14):
                _sessionCtx.RxPingCount++;
                if (_sessionCtx.RxPingCount % 100 == 0)
                    logger.Debug($"{_sessionCtx.TxPingCount} YSFP packets send");
                return;
            case "YSFS":
                logger.Debug($"{signature} {Encoding.ASCII.GetString(packet)} len={packet.Length}");
                return;
            case "YSFV":
                logger.Debug($"{signature} {Encoding.ASCII.GetString(packet)} len={packet.Length}");
                return;
            default:
                logger.Error($"Unexpected packet, signature {signature}, packet = {Convert.ToHexString(packet)}, len = {packet.Length} - packet ignored");
                return;
        }

        // From here we process a YSFD packet only

        _sessionCtx.Update(FusionCodec.Decoder.Decode(true, packet));
        
        switch (_sessionCtx.Fi)
        {
            case FrameInformation.HC:
                _sessionCtx.StreamState = StreamState.New;
                if (_sessionCtx.TransceiveMode == TransceiveMode.Rx && !_rxTimer.Enabled)
                    _rxTimer.Start();
                break;

            case FrameInformation.CC:
                _rxInactivityTicks = 0;
                if (_sessionCtx.StreamState is StreamState.End or StreamState.Idle or StreamState.Lost)
                {
                    _sessionCtx.StreamState = StreamState.New;
                    if (_sessionCtx.TransceiveMode == TransceiveMode.Rx && !_rxTimer.Enabled) 
                        _rxTimer.Start();
                }
                else
                {
                    _sessionCtx.StreamState = StreamState.Streaming;
                }
                DecompressAmbe();
                break;

            case FrameInformation.TC:
                _sessionCtx.StreamState = StreamState.End;
                break;
        }
        ExternalYsfDataConsumer?.Invoke(_sessionCtx);
    }


    /// <summary>
    /// Read YSF UDP packets sent by the reflector.
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void ReadUdpPackets()
    {
        int packetCount = 0;
        try
        {
            while (_isRunning)
            {
                try
                {
                    byte[]? packet = null;
                    if (_cfg.SimulationMode)
                    {
                        packet = _packetReader?.ReadNextPacket();
                        if (packet == null)
                            break;
                    }
                    else
                    {
                        IPEndPoint remoteEp = new(IPAddress.Any, _cfg.ReflectorPort);
                        packet = _udpClient!.Receive(ref remoteEp);
                        if (_cfg.RecordYsfPackets)
                            _packetRecorder?.WritePacket(packet);
                    }

                    packetCount++;
                    ProcessRcvdPacket(packet);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut) {}
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

            _packetReader?.Close();
            _packetReader = null;

            _packetRecorder?.Close();
            _packetRecorder = null;

            _wavRecorder?.Close();
            _wavRecorder = null;
        }
    }

    /// <summary>
    /// Processes PCM speech packets on a timer interval (~19ms).
    /// 
    /// This method is called periodically by a timer to handle received audio data:
    /// - Increments the RX watchdog counter to detect inactivity.
    /// - If inactivity exceeds threshold (100 ticks), marks the stream status as lost.
    /// - Attempts to read an audio packet from the channel reader and feeds it to the audio player.
    /// - If no packet is available and the stream is lost or ended, stops the timer and sets status to idle.
    /// </summary>
    /// <param name="sender">The timer object invoking this method (can be null).</param>
    /// <param name="e">Elapsed event arguments associated with the timer event.</param>
    private void RxTimerCallback(object? sender, ElapsedEventArgs e)
    {
        //logger.Debug("");
        //Stopwatch sw = Stopwatch.StartNew();

        if (++_rxInactivityTicks > 100) // (100 * 20ms = 2s)
        {
            _rxInactivityTicks = 0;
            _sessionCtx.StreamState = StreamState.Lost;
        }

        if (_sessionCtx.TransceiveMode is TransceiveMode.Rx && !_rxQueue.IsEmpty)
        {
            if (_rxQueue.TryDequeue(out var pcm)) // PCM data already validated
            {
                AmbeHelper.SwapPcmBytes(pcm);
                _cfg.AudioPlayer?.FeedPcmData(pcm, 6, pcm.Length - 6);
                _wavRecorder?.WritePcm(pcm!, 6, pcm!.Length - 6);
            }
            else
            {
                logger.Error("Unable to read rx queue.");
            }
        }
        else if (_sessionCtx.StreamState is StreamState.Lost or StreamState.End)
        {
            _rxInactivityTicks = 0;
            _rxTimer.Stop();
            _sessionCtx.StreamState = StreamState.Idle;
        }
        //sw.Stop();
        //logger.Debug($"{sw.ElapsedMilliseconds}");
    }

    /// <summary>
    /// Sends a UDP datagram of specified length in a thread-safe manner.
    /// </summary>
    /// <param name="dgram">The byte array containing the data to send.</param>
    /// <param name="len">The number of bytes from the array to send.</param>
    /// <remarks>
    /// This method locks access to the underlying <see cref="_udpClient"/> instance to ensure thread safety,
    /// as <see cref="UdpClient"/> is not thread-safe.
    /// </remarks>
    private void SendDatagram(byte[] dgram, int len)
    {
        lock (_lock)
        {
            _udpClient?.Send(dgram, len);
        }
    }

    /// <summary>
    /// Sends a formatted YSFP message over the network to link/login to reflector.
    /// Method is used for initial and keep-alive login.
    /// </summary>
    public void SendYsfp()
    {
        StringBuilder sb = new();
        sb.Append("YSFP").Append(_cfg.Callsign.PadRight(10));
        string cmd = sb.ToString();
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);

        _sessionCtx.TxPingCount++;
        if (_sessionCtx.TxPingCount % 100 == 0)
            logger.Debug($"{_sessionCtx.TxPingCount} YSFP packets send");
    }

    /// <summary>
    /// Sends a formatted YSFI identification message over the network.
    /// </summary>
    /// <param name="callSign">The repeater or hotspot callsign (max 10 characters, ASCII, padded with spaces).</param>
    /// <param name="rxFrequency">Receive frequency in Hz (exactly 9 digits).</param>
    /// <param name="txFrequency">Transmit frequency in Hz (exactly 9 digits).</param>
    /// <param name="locator">Optional 6-character locator (e.g., JN88EG), padded with spaces if provided.</param>
    /// <param name="town">Optional town or location name (max 20 characters, padded with spaces).</param>
    /// <param name="hotspotType">Optional hotspot type identifier (e.g., MMDVM, DV4mini), max 12 characters.</param>
    /// <param name="gwId">Optional YSF gateway ID (exactly 7 ASCII digits).</param>
    /// <exception cref="ArgumentException">Thrown if any input value violates format constraints.</exception>
    /// <remarks>
    /// The message is built according to the YSFI format specification and sent as ASCII over a UDP socket:
    /// [4] Signature "YSFI"
    /// [10] Callsign
    /// [9] RX frequency
    /// [9] TX frequency
    /// [6] Locator
    /// [20] Town
    /// [12] Hotspot type
    /// [7] Gateway ID
    /// [3] Filler (spaces)
    /// </remarks>

    private void SendYsfi(string callSign, int rxFrequency, int txFrequency, string locator, string town, string hotspotType, string gwId)
    {
        logger.Debug("");
        if (string.IsNullOrEmpty(callSign) || callSign.Length > 10)
            throw new ArgumentException("Missing or invalid arg ", nameof(callSign));
        if (rxFrequency > 999999999)
            throw new ArgumentException($"Invalid arg {rxFrequency} = {rxFrequency} Hz");
        if (txFrequency > 999999999)
            throw new ArgumentException($"Invalid arg {txFrequency} = {txFrequency} Hz");
        if (!string.IsNullOrEmpty(locator) && locator.Length > 6)
            throw new ArgumentException($"Invalid arg {nameof(locator)} = '{locator}'");
        if (!string.IsNullOrEmpty(town) && town.Length > 20)
            throw new ArgumentException($"Invalid arg {nameof(town)} = '{town}'");
        if (!string.IsNullOrEmpty(hotspotType) && hotspotType.Length > 12)
            throw new ArgumentException($"Invalid arg {nameof(hotspotType)} = '{hotspotType}'");
        if (!string.IsNullOrEmpty(gwId) && gwId.Length > 12)
            throw new ArgumentException($"Invalid arg {nameof(gwId)} = '{gwId}'");

        StringBuilder sb = new();
        sb.Append("YSFI");
        sb.Append(callSign.PadRight(10));
        sb.Append(rxFrequency.ToString("D9"));
        sb.Append(txFrequency.ToString("D9"));
        sb.Append(!string.IsNullOrEmpty(locator) ? locator.PadRight(6) : string.Empty.PadRight(6));
        sb.Append(!string.IsNullOrEmpty(town) ? town.PadRight(20) : string.Empty.PadRight(20));
        sb.Append(!string.IsNullOrEmpty(hotspotType) ? hotspotType.PadRight(12) : string.Empty.PadRight(12));
        sb.Append(!string.IsNullOrEmpty(gwId) ? gwId.PadRight(7) : string.Empty.PadRight(7));
        sb.Append("   "); // filler
        string cmd = sb.ToString();
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);
    }

    /// <summary>
    /// Sends a formatted YSFO option message over the network,
    /// containing the repeater's callsign and an optional list of DG-IDs.
    /// </summary>
    /// <param name="callSign">The callsign of the repeater (max. 10 chars).</param>
    /// <param name="dgidList">
    /// An optional list of DG-IDs to send as options. The list is serialized as a semicolon-separated string (ending with a semicolon),
    /// and the total encoded string must not exceed 36 characters.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="callSign"/> is null, empty, or longer than 10 characters,
    /// or if the DGID list string exceeds the allowed length.
    /// </exception>
    /// <remarks>
    /// The resulting message is 50 bytes long:
    /// "YSFO" (4 bytes) + padded callsign (10 bytes) + padded DGID string (36 bytes).
    /// </remarks>

    private void SendYsfo(string callSign, List<int>? dgidList)
    {
        if (string.IsNullOrEmpty(callSign) || callSign.Length > 10)
            throw new ArgumentException("Missing or invalid arg ", nameof(callSign));

        string dgidStr = "";
        if (dgidList != null)
        {
            dgidStr = string.Join(";", dgidList) + ";";
            if (dgidStr.Length > 36)
                throw new ArgumentException("Too many DGIDs; exceeds maximum allowed length.");
        }

        StringBuilder sb = new();
        sb.Append("YSFO").Append(_cfg.Callsign.PadRight(10)).Append(dgidStr.PadRight(36));
        string cmd = sb.ToString();
        logger.Debug($"{cmd}");
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);
    }

    /// <summary>
    /// Sends a YSFS message over the network to get some information about the reflector.
    /// </summary>
    private void SendYsfs()
    {
        logger.Debug("");
        string cmd = "YSFS";
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);
    }
    
    /// <summary>
    /// Sends a formatted YSFU message over the network to unlink/logout from reflector.
    /// </summary>
    private void SendYsfu()
    {
        StringBuilder sb = new();
        sb.Append("YSFU").Append(_cfg.Callsign.PadRight(10));
        string cmd = sb.ToString();
        logger.Debug($"{cmd}");
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);
    }

    /// <summary>
    /// Starts this YSF client instance and initializes all required resources.
    /// </summary>
    public void StartClient()
    {
        logger.Debug("");
        if (_isRunning)
        {
            logger.Error("This YsfClient is already running.");
            return;
        }

        if (_cfg.SimulationMode && !string.IsNullOrEmpty(_cfg.SimulationFile))
        {
            logger.Debug($"Simulation mode, input file: {_cfg.SimulationFile}");
            _packetReader = new PacketRecorder(_cfg.SimulationFile, FileMode.Open, FileAccess.Read);
            _cfg.RecordYsfPackets = false; // we never record test data
        }
        else
        {
            // Create the UDP socket used by this YSF client
            logger.Debug($"Using Reflector: {_cfg.ReflectorAddress}:{_cfg.ReflectorPort}");
            _udpClient = new UdpClient(_cfg.ReflectorAddress, _cfg.ReflectorPort);
            _udpClient.Client.ReceiveTimeout = _socketTimeout;
            if (_udpClient.Client.LocalEndPoint is not IPEndPoint localEp)
            {
                throw new Exception($"Unable to created endpoint {_cfg.ReflectorAddress}:{_cfg.ReflectorPort}");
            }
            logger.Debug($"Local socket bound to {localEp.Address}:{localEp.Port}, timeout={_socketTimeout} ms");
        }

        if (_cfg.RecordAudio && !string.IsNullOrEmpty(_cfg.RecordAudioFile))
        {
            logger.Debug($"Audio recording file: {_cfg.RecordAudioFile}");
            _wavRecorder = new(_cfg.RecordAudioFile);
        }

        if (_cfg.RecordYsfPackets && !string.IsNullOrEmpty(_cfg.RecordYsfPacketsFile))
        {
            logger.Debug($"Ysf packet recording file: {_cfg.RecordYsfPacketsFile}");
            _packetRecorder = new PacketRecorder(_cfg.RecordYsfPacketsFile, FileMode.Create, FileAccess.Write);
        }

        // Thread to read YSF UDP packets
        _udpReaderThread = new Thread(ReadUdpPackets) { IsBackground = true };
        _isRunning = true;
        _udpReaderThread.Start();

        if (!_cfg.SimulationMode)
        {
            SendYsfs();
            SendYsfp();
            SendYsfi(_cfg.Callsign, _cfg.RxFrequency, _cfg.TxFrequency, _cfg.Locator, _cfg.Town, _cfg.HotspotType, "0000000");
            //SendYsfo(_cfg.Callsign, _cfg.DgidList);
            SendYsfo(_cfg.Callsign, []);
            _pingTimer.Start();
        }
    }

    /// <summary>
    /// Stops the YSF client and releases all associated resources.
    /// </summary>
    public void StopClient()
    {
        logger.Debug($"_isRunning = {_isRunning}");
        SendYsfu();
        _isRunning = false;

        _pingTimer.Stop(); // TODO: Dispose???
        _rxTimer.Stop();
    }


    /// <summary>
    /// Starts or stops a tranmission.
    /// </summary>
    /// <param name="arg">Controls start/stop: true -> start TX, false -> stop TX</param>
    public void StartStopTransmit(bool arg)
    {
        logger.Debug($"arg={arg}");
        if (arg)
        {
            if (!_isRunning)
                throw new InvalidOperationException("The client must have been started before transmitting.");

            _rxTimer.Stop();
            _rxQueue.Clear();
            _sessionCtx.StreamState = StreamState.Transmitting;
            _sessionCtx.TxFrameNumber = 0;
            _sessionCtx.TransceiveMode = TransceiveMode.Tx;
            
            byte[] packet = FusionCodec.Encoder.EncodeHeader(true, false, _sessionCtx.TxFrameNumber, _cfg.Callsign);
            SendDatagram(packet, 155);
            _sessionCtx.TxFrameNumber++;
            //logger.Debug($"sent HC: {_clientState.TxFrameCount++}");

            _cfg.MicrophoneReader?.Start();
            _txTimer.Start();
        }
        else
        {
            _cfg.MicrophoneReader?.Stop();

            byte[] packet = FusionCodec.Encoder.EncodeHeader(true, true, _sessionCtx.TxFrameNumber, _cfg.Callsign);
            SendDatagram(packet, 155);
            _sessionCtx.TxFrameNumber++;
            //logger.Debug($"sent TC: {_clientState.TxFrameCount++}");

            _sessionCtx.TransceiveMode = TransceiveMode.Rx;
        }
    }


    /// <summary>
    /// Callback method of timer <c>_txTimer</c>. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal void TxTimerCallback(object? sender, ElapsedEventArgs e)
    {
        //_txTimer.Stop(); // test only

        if (_sessionCtx.TransceiveMode == TransceiveMode.Tx)
        {
            if (_cfg.MicrophoneReader != null && _cfg.MicrophoneReader.TryRead(ambeSpeechPacket, 6, ambeSpeechPacket.Length - 6, out int bytesRead)) { 
                AmbeHelper.SwapPcmBytes(ambeSpeechPacket); // LE -> BE
                _ambeClient.SendPacket(ambeSpeechPacket);
                byte[]? ambe = _ambeClient.ReceivePacket();
                if (AmbeHelper.IsAmbePacket(ambe))
                    _txQueue.Enqueue(ambe![6..]); // ignore 6 byte header
            }

            // TODO: consider VW mode
            if (_txQueue.Count >= 5)
            {
                //byte[] ambeFrames = new byte[5*7];
                _sessionCtx.TxAmbeData.Clear();
                for (int i = 0; i < 5; i++)
                {
                    if (_txQueue.TryDequeue(out byte[]? item) && item.Length == 7)
                    {
                        //item.CopyTo(ambeFrames, i * 7);
                        _sessionCtx.TxAmbeData.Add(item);
                    }
                    else 
                    {
                        logger.Error($"Unable to read TX queue");
                        // TODO: throw exception?
                    }
                }
                if (_sessionCtx.TransceiveMode == TransceiveMode.Tx) // late check because of thread problems
                {
                    byte[] packet = FusionCodec.Encoder.EncodeVD2(true, _sessionCtx.TxFrameNumber, _sessionCtx.TxAmbeData, _cfg.Callsign);
                    SendDatagram(packet, 155);
                    _sessionCtx.TxFrameNumber++;
                    //logger.Debug($"sent CC: {_clientState.TxFrameCount++}");
                }
            }
        }
    }    
}