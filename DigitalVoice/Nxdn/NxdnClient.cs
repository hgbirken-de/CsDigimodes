
using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using NLog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace DigitalVoice.Nxdn;

/// <summary>
/// NXDN client implementation.
/// </summary>
public sealed class NxdnClient
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte[] ambeChannelPacket = [0x61, 0x00, 0x09, 0x01, 0x01, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]; // len field: 0x0009 (7 + 2 bytes)
    
    static readonly byte[] ambeSpeechPacket = new byte[326];
    static NxdnClient()
    {
        // init AMBE specch packet
        ambeSpeechPacket[0] = 0x61;
        ambeSpeechPacket[1] = 0x01; // len field 320 + 6 - 4 = 322 -> 0x0142
        ambeSpeechPacket[2] = 0x42;
        ambeSpeechPacket[3] = 0x02;
        ambeSpeechPacket[4] = 0x00;
        ambeSpeechPacket[5] = 0xA0; // 160 PCM samples
    }

    private readonly object _lock = new();

    private readonly NxdnClientConfig _cfg;

    private readonly NxdnSessionContext _sessionCtx = new();

    UdpClient? _udpClient;
    readonly int _socketTimeout = 2000;
    bool _isRunning = false;

    PacketRecorder? _packetReader;
    PacketRecorder? _packetRecorder;

    readonly ConcurrentQueue<byte[]> _rxQueue = new();

    readonly ConcurrentQueue<byte[]> _txQueue = new();

    readonly System.Timers.Timer _pingTimer = new(3000);

    readonly System.Timers.Timer _rxTimer = new(20); // corresponds to 1 AMBE/PCM block

    readonly System.Timers.Timer _txTimer = new(20);

    Thread? _udpReaderThread; // read NXDN UDP packets

    int _rxInactivityTicks = 0;


    // NXDN data Consumer delegate
    public delegate void ConsumeNxdnData(NxdnSessionContext _sessionCtx);

    public ConsumeNxdnData? ExternalNxdnDataConsumer;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cfg"></param>
    public NxdnClient(NxdnClientConfig cfg) 
    {
        cfg.Validate();
        _cfg = cfg;

        _sessionCtx.GwId = _cfg.NxdnReflectorId;
        _sessionCtx.NxdnId = _cfg.MyNxdnId;

        _pingTimer.Elapsed += PingTimerCallback;
        _rxTimer.Elapsed += RxTimerCallback;
        _txTimer.Elapsed += TxTimerCallback;
    }

    /// <summary>
    /// Sends a list of AMBE-encoded blocks to the AMBE server for decoding (i.e. decompressing), and writes them to the output channel.
    /// Any invalid or unexpected responses are logged.
    /// </summary>
    private void DecompressAmbe()
    {
        if (_sessionCtx.RxAmbeData.Count != 4)
        {
            logger.Error($"Expecting 4 AMBE bocks to decompress, but got {_sessionCtx.RxAmbeData.Count}");
            return;
        }

        foreach (var b in _sessionCtx.RxAmbeData)
        {
            b.CopyTo(ambeChannelPacket, 6);
            //logger.Debug("AMBE: {}", Convert.ToHexString(ambeChannelPacket));
            _cfg.AmbeController!.SendPacket(ambeChannelPacket);
        }

        for (int i = 0; i < _sessionCtx.RxAmbeData.Count; i++)
        {
            byte[]? resp = _cfg.AmbeController!.ReceivePacket();
            //logger.Debug("PCM: {}", resp != null ? Convert.ToHexString(resp) : "null");
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
            [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x04, 0x31, 0x07, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x31], // *** Select custom rate for current channel
        ];

        foreach (var p in packets)
        {
            byte[]? resp = _cfg.AmbeController!.SendReceivePacket(p);
            logger.Debug("{}", resp != null ? Convert.ToHexString(resp) : "null");
        }
    }

    /// <summary>
    /// Call back method to login/ping to the NXDN server. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PingTimerCallback(object? sender, ElapsedEventArgs e)
    {
        SendNxdnp();
    }

    /// <summary>
    /// Process received NXDN packets.
    /// </summary>
    /// <param name="packet"></param>
    private void ProcessRxPacket(byte[] packet)
    {
        //logger.Debug($"{Convert.ToHexString(packet)}");
        
        // Recording
        if (_cfg.RecordRxPackets)
        {
            _packetRecorder?.WritePacket(packet);
        }

        if (Helpers.HasPrefix(packet, "NXDNP") && packet.Length == 17)
        {
            _sessionCtx.RxNxdnpCount++;
            if (_sessionCtx.RxNxdnpCount % 100 == 0) logger.Debug($"{_sessionCtx.RxNxdnpCount} NXDNP packets rcvd");
            if (_sessionCtx.StreamState is StreamState.Lost or StreamState.End)
                _sessionCtx.StreamState = StreamState.Idle;
        }
        else if (Helpers.HasPrefix(packet, "NXDND") && packet.Length == 43)
        {
            _rxInactivityTicks = 0;

            NxdnCodec.Decode(_sessionCtx, packet);

            if (_sessionCtx.Lich.Usc == NxdnUsc.SacchNs)
            {
                logger.Debug($"{NxdnUsc.SacchNs}");
                if (_sessionCtx.Eot)
                {
                    logger.Debug("Received EOT");
                    _sessionCtx.RxFrameNumber = 0;
                    _sessionCtx.StreamId = 0;
                    _sessionCtx.StreamState = StreamState.End;
                }
                else
                {
                    if (!_rxTimer.Enabled) _rxTimer.Start();
                    _sessionCtx.StreamState = StreamState.New;
                    logger.Debug($"New NXDN stream from {_sessionCtx.SrcId} to {_sessionCtx.DstId}");
                }
            }
            else if (_sessionCtx.TransceiveMode == TransceiveMode.Rx && (_sessionCtx.StreamState is StreamState.Lost or StreamState.End or StreamState.Idle))
            {
                if (!_rxTimer.Enabled) _rxTimer.Start();
                _sessionCtx.StreamState = StreamState.New;
                logger.Debug($"New NXDN stream in progress from {_sessionCtx.SrcId} to {_sessionCtx.DstId}");
            }
            else
            {
                _sessionCtx.StreamState = StreamState.Streaming;
                _sessionCtx.RxFrameNumber++;
            }
            DecompressAmbe();
            ExternalNxdnDataConsumer?.Invoke( _sessionCtx );
        }
        else
        {
            logger.Error($"Unexpected packet, len={packet.Length} {Convert.ToHexString(packet)} - packet ignored");
        }
    }

    /// <summary>
    /// Read NXDN UDP packets sent by the reflector.
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
                        IPEndPoint remoteEp = new(IPAddress.Any, _cfg.NxdnReflectorPort);
                        packet = _udpClient!.Receive(ref remoteEp);
                    }

                    packetCount++;
                    ProcessRxPacket(packet);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut) { }
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

            _cfg.WavPcmRecorder?.Close();
            _cfg.WavPcmRecorder = null;
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
                _cfg.WavPcmRecorder?.WritePcm(pcm!, 6, pcm!.Length - 6);
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
            ExternalNxdnDataConsumer?.Invoke(_sessionCtx);
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
    /// Send a NXDNU (Disconnect) message to the reflector.
    /// </summary>
    private void SendNxdnu()
    {
        byte[] buffer = new byte[17];
        Encoding.ASCII.GetBytes("NXDNU").CopyTo(buffer, 0);
        Encoding.ASCII.GetBytes(_cfg.Callsign.PadRight(10, ' ')).CopyTo(buffer, 5);

        // gwid (big endian)
        buffer[15] = (byte)((_cfg.NxdnReflectorId >> 8) & 0xFF);
        buffer[16] = (byte)((_cfg.NxdnReflectorId) & 0xFF);

        SendDatagram(buffer, buffer.Length);
    }

    /// <summary>
    /// Send a NXDNP (Ping) message to the reflector.
    /// </summary>
    private void SendNxdnp()
    {
        byte[] buffer = new byte[17];
        Encoding.ASCII.GetBytes("NXDNP").CopyTo(buffer, 0);
        Encoding.ASCII.GetBytes(_cfg.Callsign.PadRight(10, ' ')).CopyTo(buffer, 5);

        // gwid (big endian)
        buffer[15] = (byte)((_cfg.NxdnReflectorId >> 8) & 0xFF);
        buffer[16] = (byte)((_cfg.NxdnReflectorId) & 0xFF);

        SendDatagram(buffer, buffer.Length);

        _sessionCtx.TxNxdnpCount++;
        if (_sessionCtx.TxNxdnpCount % 100 == 0)
            logger.Debug($"{_sessionCtx.TxNxdnpCount} NXDNP packets sent");
    }

    /// <summary>
    /// Starts this NXDN client instance and initializes all required resources.
    /// </summary>
    public void StartClient()
    {
        logger.Debug("");

        if (_isRunning)
        {
            logger.Error("This NxdnClient is already running.");
            return;
        }

        _cfg.AmbeController!.Open();

        InitDV3000();

        if (_cfg.SimulationMode && !string.IsNullOrEmpty(_cfg.SimulationModeFile))
        {
            logger.Debug($"Simulation mode, input file: {_cfg.SimulationModeFile}");
            _packetReader = new PacketRecorder(_cfg.SimulationModeFile, FileMode.Open, FileAccess.Read);
            _cfg.RecordRxPackets = false; // we never record test data
        }
        else
        {
            // Create the UDP socket used by this NXDN client
            logger.Debug($"Using Server: {_cfg.NxdnReflectorAddr}:{_cfg.NxdnReflectorPort}");
            _udpClient = new UdpClient(_cfg.NxdnReflectorAddr, _cfg.NxdnReflectorPort);
            _udpClient.Client.ReceiveTimeout = _socketTimeout;
            if (_udpClient.Client.LocalEndPoint is not IPEndPoint localEp)
            {
                throw new Exception($"Unable to created endpoint {_cfg.NxdnReflectorAddr} : {_cfg.NxdnReflectorPort}");
            }
            logger.Debug($"Local socket bound to {localEp.Address}:{localEp.Port}, timeout={_socketTimeout} ms");
        }

        if (_cfg.RecordRxPackets && !string.IsNullOrEmpty(_cfg.RecordRxPacketsFile))
        {
            logger.Debug($"NXDN packet recording file: {_cfg.RecordRxPacketsFile}");
            _packetRecorder = new PacketRecorder(_cfg.RecordRxPacketsFile, FileMode.Create, FileAccess.Write);
        }

        // Thread to read NXDN UDP packets
        _udpReaderThread = new Thread(ReadUdpPackets) { IsBackground = true };
        _isRunning = true;
        _udpReaderThread.Start();

        if (!_cfg.SimulationMode)
        {
            SendNxdnp();
            _pingTimer.Start();
        }
    }

    /// <summary>
    /// Stops this NXDN client and releases all associated resources.
    /// </summary>
    public void StopClient()
    {
        logger.Debug($"_isRunning = {_isRunning}");
        SendNxdnu();
        _isRunning = false;

        _sessionCtx.StreamId = 0;
        _sessionCtx.StreamState = StreamState.End;

        _pingTimer.Stop(); // TODO: Dispose???
        _rxTimer.Stop();

        _cfg.AmbeController!.Close();

        ExternalNxdnDataConsumer?.Invoke(_sessionCtx);
    }

    /// <summary>
    /// Starts or stops a tranmission.
    /// </summary>
    /// <param name="arg">Controls start/stop: true -> start TX, false -> stop TX</param>
    public void StartStopTransmit(bool arg)
    {
        logger.Debug($"arg={arg}"); 
        if (!_isRunning) throw new InvalidOperationException("The client must have been started before transmitting.");
        
        if (arg)
        {
            _rxTimer.Stop();
            _rxQueue.Clear();
            _sessionCtx.Eot = false;
            _sessionCtx.StreamState = StreamState.Transmitting;
            _sessionCtx.TransceiveMode = TransceiveMode.Tx;
            _sessionCtx.TxFrameNumber = 0;

            byte[] frame = NxdnCodec.EncodeFrame(_sessionCtx);
            SendDatagram(frame, frame.Length);
            logger.Debug($"Sent header: {_sessionCtx.TxFrameNumber}");
            _sessionCtx.TxFrameNumber++;

            _cfg.MicrophoneReader?.Start();
            _txTimer.Start();
        }
        else
        {
            _cfg.MicrophoneReader?.Stop();
            _txTimer.Stop();
            _txQueue.Clear();
            
            _sessionCtx.Eot = true;

            byte[] frame = NxdnCodec.EncodeFrame(_sessionCtx);
            SendDatagram(frame, frame.Length);
            logger.Debug($"Sent header: {_sessionCtx.TxFrameNumber}");
            _sessionCtx.TxFrameNumber++;

            _sessionCtx.StreamState = StreamState.End;
            _sessionCtx.TransceiveMode = TransceiveMode.Rx;

            ExternalNxdnDataConsumer?.Invoke( _sessionCtx );
        }
    }

    /// <summary>
    /// Callback method of timer <c>_txTimer</c>. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal void TxTimerCallback(object? sender, ElapsedEventArgs e)
    {

        if (_sessionCtx.TransceiveMode != TransceiveMode.Tx)
            return;

        if (_cfg.MicrophoneReader != null && _cfg.MicrophoneReader.TryRead(ambeSpeechPacket, 6, ambeSpeechPacket.Length - 6, out int bytesRead))
        {
            AmbeHelper.SwapPcmBytes(ambeSpeechPacket); // LE -> BE
            _cfg.AmbeController.SendPacket(ambeSpeechPacket);
            byte[]? ambe = _cfg.AmbeController.ReceivePacket();
            if (AmbeHelper.IsAmbePacket(ambe))
                _txQueue.Enqueue(ambe![6..]); // ignore 6 byte header
        }

        // TODO: consider VW mode

        const int n = 4; // 4 AMBE bocks per NXDND frame
        if (_txQueue.Count >= n)
        {
            for (int i = 0; i < n; i++)
            {
                if (_txQueue.TryDequeue(out byte[]? item) && item.Length == 7)
                {
                    item.CopyTo(_sessionCtx.TxAmbeData, i * 7);
                }
                else
                {
                    logger.Error($"Unable to read TX queue");
                    // TODO: throw exception?
                }
            }
            if (_sessionCtx.TransceiveMode == TransceiveMode.Tx) // late check to prevent thread related problems
            {
                byte[] nxdxdFrame = NxdnCodec.EncodeFrame(_sessionCtx);
                SendDatagram(nxdxdFrame, nxdxdFrame.Length);
                logger.Debug($"Sent voice frame: {_sessionCtx.TxFrameNumber}");
                _sessionCtx.TxFrameNumber++;
            }
        }

    }
}