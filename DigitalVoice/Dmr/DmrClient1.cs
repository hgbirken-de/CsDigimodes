using AmbeServer;
using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using NLog;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace DigitalVoice.Dmr;

/// <summary>
/// DMR client implementing the Homebrew Protocol Generic (char oriented).
/// See <see href="https://wiki.brandmeister.network/index.php/Homebrew_repeater_protocol/Spec">Homebrew repeater protocol/Spec</see> for details.
/// </summary>
public sealed class DmrClient1
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte[] ambeChannelPacket = [0x61, 0x00, 0x0b, 0x01, 0x01, 0x48, 0, 0, 0, 0, 0, 0, 0, 0, 0]; // 15 bytes

    static readonly byte[] ambeSpeechPacket = new byte[326];

    readonly DmrClientConfig _cfg;

    UdpClient? _udpClient;

    bool _isRunning = false;

    Thread? _dmrPacketReaderThread; // read DMR UDP packets

    Status _status = Status.Disconnected;

    string? _salt;

    readonly object _lock = new();

    AmbeClient? _ambeClient;

    AMBE3000RController? _ambe3000RController;

    readonly ConcurrentQueue<byte[]> _rxQueue = new();
    readonly ConcurrentQueue<byte[]> _txQueue = new();

    readonly DmrSessionContext _clientState;

    PacketRecorder? _packetRecorder;
    PacketRecorder? _packetReader;

    WavPcmRecorder? _wavRecorder;

    readonly System.Timers.Timer _pingTimer = new(10000);

    readonly System.Timers.Timer _rxTimer = new(20); // correponds to 1 AMBE blocks a 20ms

    readonly System.Timers.Timer _txTimer = new(20);

    int _rxInactivityCount = 0;

    // DMR data Consumer delegate
    public delegate void ConsumeDmrData(DmrSessionContext state);

    public ConsumeDmrData? ExternalDmrDataConsumer;

    static DmrClient1()
    {
        new byte[] { 0x61, 0x01, 0x42, 0x02, 0x00, 0xA0 }.CopyTo(ambeSpeechPacket, 0);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="srvAddr">The Brandmeister server IP address.</param>
    /// <param name="port">The Brandmeister server port number.</param>
    /// <param name="callsign">My callsign</param>
    /// <param name="password">My password.</param>
    /// <param name="dmrId">My DMR id.</param>
    /// <param name="essid">Extended subscriber ID (0 thru 99)</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public DmrClient1(DmrClientConfig cfg)
    {
        cfg.Validate();
        _cfg = cfg.DeepClone();

        _clientState = new()
        {
            Protocol = DmrProtocol.Homebrew,
            TxColorCode = _cfg.ColorCode,
            TxSrcId = _cfg.MyDmrId,
            TxRptId = _cfg.EssId > 0 ? _cfg.MyDmrId * 100 + _cfg.EssId : _cfg.MyDmrId,
            TxTimeSlot = _cfg.TimeSlot
        };

        _pingTimer.Elapsed += PingTimerCallback;
        _rxTimer.Elapsed += RxTimerCallback;
        _txTimer.Elapsed += TxTimerCallback;
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
            [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x04, 0x31, 0x07, 0x54, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6f, 0x48], // Select custom rate for current channel
        ];

        foreach (var p in packets)
        {
            byte[]? resp = _cfg.AmbeServiceType switch
            {
                AmbeServiceType.Server => _ambeClient?.SendReceivePacket(p),
                AmbeServiceType.Stick => _ambe3000RController?.SendReceivePacket(p),
                _ => throw new ArgumentOutOfRangeException(nameof(_cfg.AmbeServiceType), _cfg.AmbeServiceType, "AMBE Service type not defined"),
            };

            logger.Debug($"send: {Convert.ToHexString(p)}");
            logger.Debug("rcvd: {}", resp != null ? Convert.ToHexString(resp) : "null");
        }
    }

    /// <summary>
    /// Deconmpress DMR AMBE data. The decompression result is written to the internal rx queue <see cref="_rxQueue"/>.
    /// </summary>
    /// <param name="dmr3Ambe">3 AMBE frames with 9 bytes each</param>
    /// <exception cref="ArgumentException"></exception>
    private void DecompressAmbe(byte[] dmr3Ambe)
    {
        if (dmr3Ambe == null)
            throw new ArgumentException("Argument must not be null!");

        if (dmr3Ambe.Length != 27)
            throw new ArgumentException($"Invalid AMBE data: {Convert.ToHexString(dmr3Ambe)}");

        const int n = 3; // 3 AMBE frames per DMR frame 
        for (int i = 0, offset = 0; i < n; i++, offset += 9)
        {
            Buffer.BlockCopy(dmr3Ambe, offset, ambeChannelPacket, 6, 9);
            switch (_cfg.AmbeServiceType)
            {
                case AmbeServiceType.Server: _ambeClient?.SendPacket(ambeChannelPacket); break;
                case AmbeServiceType.Stick: _ambe3000RController?.SendPacket(ambeChannelPacket); break;
            }
        }

        for (int i = 0; i < n; i++)
        {
            byte[]? pcm = _cfg.AmbeServiceType switch
            {
                AmbeServiceType.Server => _ambeClient?.ReceivePacket(),
                AmbeServiceType.Stick => _ambe3000RController?.ReceivePacket(),
                _ => throw new InvalidOperationException($"AMBE Service type not defined: {_cfg.AmbeServiceType}"),
            };
            if (AmbeHelper.IsSpeechPacket(pcm))
            {
                _rxQueue.Enqueue(pcm!);
            }
            else
            {
                logger.Error("Invalid PCM data: {}", pcm != null ? Convert.ToHexString(pcm) : "null");
            }
        }
    }


    /// <summary>
    /// Call back method to login the FCS reflector (keep alive). 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PingTimerCallback(object? sender, ElapsedEventArgs e)
    {
        SendPing2();
    }


    /// <summary>
    /// Parses a raw DMRD (DMR Data) packet and updates the <c>_modeInfo</c> structure accordingly.
    /// </summary>
    /// <param name="dmrPkt">
    /// Byte array representing a DMRD packet with the following layout:
    ///   [0-3]   : ASCII header "DMRD",
    ///   [4]     : Frame number,
    ///   [5-7]   : Source DMR ID (24-bit, BE),
    ///   [8-10]  : Destination DMR ID (24-bit, BE),
    ///   [11-14] : Repeater/Gateway ID (32-bit, BE),
    ///   [15]    : Flag bits (slot, call type, frame type, sequence),
    ///   [16-19] : Stream ID (32-bit, BE),
    ///   [20-52] : AMBE voice payload (33 bytes: 3 AMBE frames + sync info),
    /// </param>
    /// <remarks>
    /// Extracts key identifiers and state from the packet, such as source/destination IDs, call type, slot number,
    /// frame type, and stream ID. Updates the internal <c>_modeInfo</c> state accordingly and logs the decoded values.
    /// 
    /// If the frame type indicates voice or voice-sync, stream status is set to <see cref="StreamState.Streaming" />.
    /// If it is a data-sync frame with a specific sequence number, the stream is marked as new or ended.
    /// </remarks>

    private void ProcessDmrdPacket(byte[] dmrPkt)
    {
        byte[] dmr3Ambe = DmrCodec.DecodeDmrFrame(_clientState, dmrPkt);
        
        switch (_clientState.RxFrameType)
        {
            case FrameType.Voice:
            case FrameType.VoiceSync:
                if (_clientState.RxStreamState is StreamState.End or StreamState.Idle or StreamState.Lost)
                {
                    _clientState.RxStreamState = StreamState.New;
                    _rxTimer.Start();
                }
                else
                {
                    _clientState.RxStreamState = StreamState.Streaming;
                }

                DecompressAmbe(dmr3Ambe);
                _rxInactivityCount = 0;
                break;
            case FrameType.DataSync:
                if (_clientState.RxVoiceOrDataSeq == 1)
                {
                    logger.Debug($"New DMR stream from {_clientState.RxSrcId} to {_clientState.RxDstId}");
                    _clientState.RxStreamState = StreamState.New;
                    _rxTimer.Start();
                }
                else if (_clientState.RxVoiceOrDataSeq == 2)
                {
                    _clientState.RxStreamState = StreamState.End;
                }
                break;
        }
        ExternalDmrDataConsumer?.Invoke(_clientState);
    }


    /// <summary>
    /// Processes a received DMR packet and updates the internal state machine accordingly.
    /// </summary>
    /// <param name="dmrPkt">The raw DMR packet data received from the UDP socket.</param>
    private void ProcessRcvdDmrPacket(byte[] dmrPkt)
    {
        if (logger.IsTraceEnabled)
            logger.Trace($"rcvd {dmrPkt.Length} bytes: {Convert.ToHexString(dmrPkt)}");
        _packetRecorder?.WritePacket(dmrPkt);

        if (Helpers.HasPrefix(dmrPkt, "DMRD") && dmrPkt.Length == 53)
        {
            ProcessDmrdPacket(dmrPkt);
        }
        else if (Helpers.HasPrefix(dmrPkt, "MSTACK"))
        {
            switch (_status)
            {
                case Status.WaitingLogin:
                    if (dmrPkt.Length == 22)
                    {
                        _salt = Encoding.ASCII.GetString(dmrPkt, 14, 8);
                        SendAuthentication();
                        _status = Status.WaitingAuthentication;
                    }
                    else
                    {
                        logger.Error($"Unable to extract salt: {dmrPkt.Length} {Convert.ToHexString(dmrPkt)}");
                        // TODO: how to continue???
                    }
                    break;
                case Status.WaitingAuthentication:
                    _status = Status.WaitingConfig;
                    SendConfig();
                    break;
                case Status.WaitingConfig:
                    _status = Status.Running;
                    if (!_cfg.SimulationMode)
                        _pingTimer.Start(); // start the ping only after the client is in Running status
                    break;
            }
        }
        else if (Helpers.HasPrefix(dmrPkt, "MSTNAK"))
        {
            logger.Error("MSTNAK");
        }
        else if (Helpers.HasPrefix(dmrPkt, "MSTCL"))
        {
            logger.Error("MSTCL");
        }
        else if (Helpers.HasPrefix(dmrPkt, "MSTPONG") || Helpers.HasPrefix(dmrPkt, "RPTPONG"))
        {
            _clientState.RxPongCount++;
            if (_clientState.RxPongCount % 60 == 0)
                logger.Debug($"{_clientState.RxPongCount} PONG rcvd");
        }
        else if (Helpers.HasPrefix(dmrPkt, "RPTACK"))
        {
            switch (_status) {
                case Status.WaitingAuthentication:
                    _status = Status.WaitingConfig;
                    SendConfig();
                    break;
                case Status.WaitingConfig:
                    _status = Status.Running;
                    if (!_cfg.SimulationMode)
                        _pingTimer.Start(); // start the ping only after the client is in Running status
                    break;
                case Status.WaitingLogin:
                    if (dmrPkt.Length > 13)
                    {
                        // TODO: len check???
                        _salt = Encoding.ASCII.GetString(dmrPkt, 6, 8);
                        SendAuthentication();
                        _status = Status.WaitingAuthentication;
                    }
                    else
                    {
                        logger.Error($"Unable to extract salt: {dmrPkt.Length} {Convert.ToHexString(dmrPkt)}");
                        // TODO: how to continue???
                    }
                    break;
            }
        }
        else
        {
            logger.Error($"Unexpected DMR packet: Len={dmrPkt.Length}  {Convert.ToHexString(dmrPkt)}");
        }
    }

    /// <summary>
    /// Continuously reads DMR UDP packets from the network and dispatches them for processing.
    /// </summary>
    /// <remarks>
    /// This method runs in a loop while the <c>_isRunning</c> flag is <c>true</c>. It receives UDP datagrams from the configured 
    /// UDP client, logs the received data in hexadecimal format, and forwards each packet to <see cref="ProcessRcvdDmrPacket"/> 
    /// for protocol handling.
    ///
    /// If <c>_simulationMode</c> is enabled, the method throws a <see cref="NotImplementedException"/> since simulation is not yet implemented.
    ///
    /// All exceptions are caught and logged. Upon exit, it logs the total number of packets received and attempted to process.
    /// </remarks>
    private void ReadUdpPackets()
    {
        int packetCount = 0;
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
                    IPEndPoint remoteEp = new(IPAddress.Any, _cfg.BmServerPort);
                    packet = _udpClient!.Receive(ref remoteEp);
                }
                packetCount++;
                ProcessRcvdDmrPacket(packet);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                if (logger.IsTraceEnabled)
                    logger.Trace($"timeout");
                // Optionally sleep or backoff slightly here
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                break; // game over
            }
        }
        _packetReader?.Close();
        _packetReader = null;
        _packetRecorder?.Close();
        _packetRecorder = null;
        _wavRecorder?.Close();
        _wavRecorder = null;
        logger.Debug($"packetCount = {packetCount}");
    }


    /// <summary>
    /// Processes incoming speech packets on a timer interval (~19ms).
    /// 
    /// This method is called periodically by a timer to handle received audio data:
    /// - Increments the RX inactivity counter to detect inactivity.
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

        if (++_rxInactivityCount > 33) // 33 * 60ms ~ 2sec
        {
            _rxInactivityCount = 0;
            _clientState.TxStreamId = 0;
            _clientState.RxStreamState = StreamState.Lost;
            ExternalDmrDataConsumer?.Invoke(_clientState);
        }

        if (!_rxQueue.IsEmpty) // PCM data
        {
            if (_rxQueue.TryDequeue(out byte[]? pcm)) // PCM data already validated
            {
                AmbeHelper.SwapPcmBytes(pcm!);
                _cfg.AudioPlayer?.FeedPcmData(pcm!, 6, pcm!.Length - 6);
                _wavRecorder?.WritePcm(pcm!, 6, pcm!.Length - 6);
            }
            else
            {
                logger.Error("Unable to read rx queue");
                return;
            }
        }
        else if (_clientState.RxStreamState is StreamState.Lost or StreamState.End)
        {
            _rxInactivityCount = 0;
            _rxTimer.Stop();
            _clientState.TxStreamId = 0;
            _clientState.RxStreamState = StreamState.Idle;
            //logger.Debug("Queue empty");
        }
        //sw.Stop();
        //logger.Debug($"{sw.ElapsedMilliseconds}");
    }

    /// <summary>
    /// Sends an authentication message to the BrandMeister server using the Homebrew protocol.
    /// </summary>
    /// <remarks>
    /// Constructs the authentication payload by combining the RPTK prefix, the 8-digit hexadecimal representation
    /// of the DMR ID (optionally including the ESSID), and a SHA-256 hash of the salt concatenated with the password.
    /// The resulting ASCII message is sent via UDP to the configured server.
    /// </remarks>
    private void SendAuthentication()
    {
        logger.Debug("");
        StringBuilder sb = new();
        sb.Append("RPTK").Append(_cfg.FullDmrId.ToString("X8")).Append(Helpers.GetSha256Hash(_salt + _cfg.Password)); // 76 bytes
        byte[] dgram = Encoding.ASCII.GetBytes(sb.ToString());
        SendDatagram(dgram, dgram.Length);
    }


    /// <summary>
    /// Send the argument bytes to the Brandmeister server. 
    /// </summary>
    /// <param name="dgram">The bytes to send.</param>
    /// <param name="len">The length of the datagram.</param>
    private void SendDatagram(byte[] dgram, int len)
    {
        lock(_lock)
        {   // UdpClient is not thread safe
            _udpClient?.Send(dgram, len);
        }
    }


    /// <summary>
    /// Constructs and sends the configuration datagram ("RPTC") to the DMR server.
    /// </summary>
    private void SendConfig()
    {
        logger.Debug("");
        StringBuilder sb = new(306); // pre-allocate target size
        sb.Append("RPTC");
        sb.Append(Helpers.FormatField(_cfg.Callsign, 8));
        sb.Append(_cfg.FullDmrId.ToString("X8"));
        sb.Append(_cfg.Frequency.ToString("D9"));
        sb.Append(_cfg.Frequency.ToString("D9"));
        sb.Append(_cfg.TxPower.ToString("D2"));
        sb.Append(_cfg.ColorCode.ToString("D2"));
        sb.Append(_cfg.Latitude.ToString("+00.0000;-00.0000", CultureInfo.InvariantCulture));
        sb.Append(_cfg.Longitude.ToString("+000.0000;-000.0000", CultureInfo.InvariantCulture));
        sb.Append(_cfg.Height.ToString("D3"));
        sb.Append(Helpers.FormatField(_cfg.Location, 20));
        sb.Append(Helpers.FormatField(_cfg.Description, 20));
        sb.Append(Helpers.FormatField(_cfg.Url, 124));
        sb.Append(Helpers.FormatField(_cfg.VersionInfo, 40));
        sb.Append(Helpers.FormatField(_cfg.PlatformTag, 40));

        byte[] dgram = Encoding.ASCII.GetBytes(sb.ToString());
        SendDatagram(dgram, dgram.Length);
    }

    /// <summary>
    /// Sends a DMR close/logout packet to the server via UDP.
    /// </summary>
    /// <remarks>
    /// Constructs a disconnect message in the format "RPTCLXXXXXXXX", where "XXXXXXXX" is the 8-character
    /// hexadecimal representation of the DMR ID with optional ESSID offset.
    /// The message is sent as an ASCII-encoded UDP packet.
    /// </remarks>
    private void SendClose()
    {
        logger.Debug("");
        byte[] dgram = Encoding.ASCII.GetBytes($"RPTCL{_cfg.FullDmrId:X8}");
        SendDatagram(dgram, dgram.Length);
    }

    /// <summary>
    /// Sends a DMR login packet to the server via UDP.
    /// </summary>
    /// <remarks>
    /// Constructs a login message in the format "RPTLXXXXXXXX", where "XXXXXXXX" is the 8-character
    /// hexadecimal representation of the DMR ID with optional ESSID offset.
    /// The message is encoded as ASCII and sent as a UDP packet.
    /// </remarks>
    private void SendLogin()
    {
        logger.Debug("");
        byte[] dgram = Encoding.ASCII.GetBytes($"RPTL{_cfg.FullDmrId:X8}");
        SendDatagram(dgram, dgram.Length);
    }

    /// <summary>
    /// Sends a DMR ping packet to the server via UDP.
    /// </summary>
    /// <remarks>
    /// Constructs a ping message in the format "RPTPINGXXXXXXXX", where "XXXXXXXX" is the 8-character
    /// hexadecimal representation of the DMR ID, optionally including the ESSID if configured.
    /// The message is encoded using ASCII and sent as a UDP datagram.
    /// </remarks>
    private void SendPing()
    {
        logger.Debug("");
        byte[] dgram = Encoding.ASCII.GetBytes($"RPTPING{_cfg.FullDmrId:X8}");
        SendDatagram(dgram, dgram.Length);
    }

    /// <summary>
    /// Sends a Master ping packet to the server via UDP.
    /// </summary>
    /// <remarks>
    /// Constructs a ping message in the format "MSTPINGXXXXXXXX", where "XXXXXXXX" is the 8-character
    /// hexadecimal representation of the DMR ID, optionally including the ESSID if configured.
    /// The message is encoded using ASCII and sent as a UDP datagram.
    /// </remarks>
    private void SendPing2()
    {
        byte[] dgram = Encoding.ASCII.GetBytes($"MSTPING{_cfg.FullDmrId:X8}");
        SendDatagram(dgram, dgram.Length);

        _clientState.TxPingCount++;
        if (_clientState.TxPingCount % 100 == 0)
            logger.Debug($"{_clientState.TxPingCount} MSTPING packets send");
    }

    /// <summary>
    /// Set the TX destination talkgroup.
    /// </summary>
    /// <param name="dmrId">DMR id of the target talkgroup.</param>
    /// <param name="flco">Full link control op-code of the target talkgroup.</param>
    public void SetTxDst(int dmrId, Flco flco)
    {
        _clientState.TxDstId = dmrId;
        _clientState.TxFlco = flco;
    }

    /// <summary>
    /// Starts this DMR client running.
    /// </summary>
    public void Start()
    {
        logger.Debug($"{_isRunning}");
        if (_isRunning)
        {
            logger.Error("Client is already running");
            return;
        }

        switch (_cfg.AmbeServiceType)
        {
            case AmbeServiceType.Server:
                _ambeClient = new(_cfg.AmbeServerAddr, _cfg.AmbeServerPort);
                break;
            case AmbeServiceType.Stick:
                _ambe3000RController = new(_cfg.AmbeStickComport);
                _ambe3000RController.Open();
                break;
        }

        InitDV3000();

        string dataDir = "data";
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        if (_cfg.SimulationMode)
        {
            _packetReader = new PacketRecorder(_cfg.SimulationModeFile!, FileMode.Open, FileAccess.Read);
            _cfg.RecordDmrPackets = false; // we never record a sim file
        }
        else
        {
            try
            {
                // Resolve hostname synchronously
                logger.Debug($"Connect to server: {_cfg.BmServerAddress}:{_cfg.BmServerPort}");
                IPAddress[] addresses = Dns.GetHostAddresses(_cfg.BmServerAddress);
                if (addresses.Length == 0)
                {
                    logger.Error($"No IP addresses found for hostname '{_cfg.BmServerAddress}'");
                    return;
                }

                _udpClient = new UdpClient(_cfg.BmServerAddress, _cfg.BmServerPort);
                _udpClient.Client.ReceiveTimeout = _cfg.SocketTimeout;
                if (_udpClient.Client.LocalEndPoint is not IPEndPoint localEp)
                {
                    throw new Exception($"Unable to created endpoint, {_cfg.BmServerAddress}:{_cfg.BmServerPort}");
                }
                logger.Debug($"Local socket bound to {localEp.Address}:{localEp.Port}, timeout={_cfg.SocketTimeout} ms");
            }
            catch (SocketException ex)
            {
                logger.Error($"{ex.Message}, ErrorCode = {ex.ErrorCode}");
                _udpClient?.Dispose();
                throw new Exception($"Unable to connect to BM server at {_cfg.BmServerAddress}:{_cfg.BmServerPort}", ex);
            }
        }

        if (_cfg.RecordAudio && !string.IsNullOrEmpty(_cfg.RecordAudioFile))
        {
            _wavRecorder = new(_cfg.RecordAudioFile);
        }

        if (_cfg.RecordDmrPackets && !string.IsNullOrEmpty(_cfg.RecordDmrPacketsFile))
        {
            _packetRecorder = new PacketRecorder(_cfg.RecordDmrPacketsFile, FileMode.Create, FileAccess.Write);
        }

        _isRunning = true;
        _dmrPacketReaderThread = new Thread(ReadUdpPackets) { IsBackground = true };
        _dmrPacketReaderThread.Start();
        
        _status = Status.WaitingLogin;
        SendLogin();
    }

    /// <summary>
    /// Stops this DMR client running.
    /// </summary>
    public void Stop()
    {
        logger.Debug($"{_isRunning}");
        _isRunning = false;
        if (_pingTimer.Enabled)
            _pingTimer.Stop();
        if (_rxTimer.Enabled)
            _rxTimer.Stop();
        if (_txTimer.Enabled)
            _txTimer.Stop();

        if (!_cfg.SimulationMode)
            SendClose();

        _ambe3000RController?.Close();
        _ambe3000RController = null;
        _ambeClient = null;
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
            if (!_isRunning)
                throw new InvalidOperationException("The client must be started before transmitting.");

            _clientState.TransceiveMode = TransceiveMode.Tx;

            // TODO: handle PING timer?

            _rxTimer.Stop();
            _rxQueue.Clear();
            _clientState.RxStreamState = StreamState.Idle;

            // Create and send initial header frame
            _clientState.TxFrameCount = 0;
            byte[] headerFrame = DmrCodec.CreateHeaderFrame(_clientState, false);
            SendDatagram(headerFrame, headerFrame.Length);
            _clientState.TxFrameCount++;

            _cfg.MicrophoneReader?.Start();
            _txTimer.Start();
        }
        else
        {
            // Create EOT header frame
            byte[] headerFrame = DmrCodec.CreateHeaderFrame(_clientState, true); // EOT header
            SendDatagram(headerFrame, headerFrame.Length);
            _clientState.TxFrameCount++;

            _clientState.TransceiveMode = TransceiveMode.Rx;
            _txTimer.Stop();
            _cfg.MicrophoneReader?.Stop();
            _txQueue.Clear();

            _clientState.RxStreamState = StreamState.Idle;
        }
    }


    /// <summary>
    /// TX timer callback method to periodically capture microphone audio, encodes it via the AMBE codec, 
    /// and transmits DMR voice frames when in transmit mode.
    /// </summary>
    /// <param name="sender">The timer object triggering the callback.</param>
    /// <param name="e">The elapsed event arguments.</param>
    internal void TxTimerCallback(object? sender, ElapsedEventArgs e)
    {
        //logger.Debug($"_clientState = {_clientState}");

        if (_clientState.TransceiveMode != TransceiveMode.Tx)
            return;

        int frameStartCount = _clientState.TxFrameCount; // snapshot

        //_clientState.RxStreamState = StreamState.Transmitting; // muss das sein?

        // TODO: _cfg.MicrophoneReader == null  muss früher geprüft werden, hier zu spät!!!
        if (_cfg.MicrophoneReader == null || !_cfg.MicrophoneReader.TryRead(ambeSpeechPacket, 6, ambeSpeechPacket.Length - 6, out int bytesRead))
            return;

        AmbeHelper.SwapPcmBytes(ambeSpeechPacket); // LE -> BE format
        switch (_cfg.AmbeServiceType)
        {
            case AmbeServiceType.Server: _ambeClient?.SendPacket(ambeSpeechPacket); break;
            case AmbeServiceType.Stick: _ambe3000RController?.SendPacket(ambeSpeechPacket); break;
        }
        byte[]? ambe = _cfg.AmbeServiceType switch
        {
            AmbeServiceType.Server => _ambeClient?.ReceivePacket(),
            AmbeServiceType.Stick => _ambe3000RController?.ReceivePacket(),
            _ => throw new InvalidOperationException($"AMBE Service type not defined: {_cfg.AmbeServiceType}"),
        };
        if (AmbeHelper.IsAmbePacket(ambe))
        {
            _txQueue.Enqueue(ambe[6..]);
        }
        else
        {
            logger.Error($"Unexpected response of AMBE server: {Convert.ToHexString(ambe)}");
            return;
        }

        if (_txQueue.Count >= 3)
        {
            byte[] ambeFrames = new byte[3 * 9];
            for (int i = 0; i < 3; i++)
            {
                if (_txQueue.TryDequeue(out byte[]? obj) && obj.Length == 9)
                {
                    obj.CopyTo(ambeFrames, i * 9);
                }
                else
                {
                    logger.Error($"Unable to read TX queue");
                    // TODO: throw exception or return???
                }
            }

            if (_clientState.TransceiveMode == TransceiveMode.Tx && _clientState.TxFrameCount == frameStartCount) // prevent thread related problems
            {
                byte[] voiceFrame = DmrCodec.CreateVoiceFrame(_clientState, ambeFrames);
                SendDatagram(voiceFrame, voiceFrame.Length);
                _clientState.TxFrameCount++;
                ExternalDmrDataConsumer?.Invoke(_clientState);
            }
        }
    }

}