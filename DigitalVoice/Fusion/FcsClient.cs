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

using static DigitalVoice.Common.Helpers;

namespace DigitalVoice.Fusion;

/// <summary>
/// FCS client implementation.
/// https://ycs-wiki.xreflector.net/doku.php?id=start:protocols:fcs
/// </summary>
public class FcsClient
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte[] ambeChannelPacket = [0x61, 0x00, 0x09, 0x01, 0x01, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]; // 13 bytes 

    static readonly byte[] ambeSpeechPacket = new byte[326];

    readonly object _lock = new();

    readonly FcsClientConfig _cfg;

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

    readonly System.Timers.Timer _rxTimer = new(20);  // corresponds to 1 AMBE block (20ms)
    
    readonly System.Timers.Timer _txTimer = new(20);

    Thread? _udpReaderThread; // read FCS UDP packets

    readonly YsfSessionContext _sessionCtx = new(false);

    int _rxInactivityTicks = 0;

    // FCS data Consumer delegate
    public delegate void ConsumeFcsData(YsfSessionContext fcsClientState);

    public ConsumeFcsData? ExternalFcsDataConsumer;

    static FcsClient()
    {
        // init AMBE speech packet
        ambeSpeechPacket[0] = 0x61;
        ambeSpeechPacket[1] = 0x01; // len field 320 + 6 - 4 = 322 -> 0x0142
        ambeSpeechPacket[2] = 0x42;
        ambeSpeechPacket[3] = 0x02;
        ambeSpeechPacket[4] = 0x00;
        ambeSpeechPacket[5] = 0xA0; // 160 PCM samples
    }


    /// <summary>
    /// Constructor that creates a new FcsClient instance.
    /// </summary>
    /// <param name="cfg">Configuration parameters for this FCS client instance.</param>
    public FcsClient(FcsClientConfig cfg)
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
    /// Call back method to login the FCS reflector (keep alive). 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PingTimerCallback(object? sender, ElapsedEventArgs e)
    {
        SendPing();
    }

    /// <summary>
    /// Process a received FCS packet.
    /// </summary>
    /// <param name="packet">The full FCS packet.</param>
    public void ProcessRcvdPacket(byte[] packet)
    {
        //logger.Debug($"len={packet.Length} {Convert.ToHexString(packet)}");
        if (packet.Length == 130)
        {
            // payload -> processed below
        }
        else if (HasPrefix(packet, "ACK") && packet.Length == 3)
        {
            logger.Debug($"{Encoding.ASCII.GetString(packet)} len={packet.Length}");
            return;
        }
        else if (HasPrefix(packet, "ONLINE") && packet.Length is 7 or 10)
        {
            _sessionCtx.RxPingCount++;
            if (_sessionCtx.RxPingCount % 100 == 0)
                logger.Debug($"{_sessionCtx.RxPingCount} ONLINE packets rcvd");
            return;
        }
        else 
        {
            logger.Debug($"Unexpected FCS packet rcvd: len={packet.Length} {Convert.ToHexString(packet)}");
            return;
        }

        //
        // From here we process a payload packet exclusively
        //

        _sessionCtx.Update(FusionCodec.Decoder.Decode(false, packet));
        logger.Debug($"{_sessionCtx}");

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
        ExternalFcsDataConsumer?.Invoke(_sessionCtx);
    }


    /// <summary>
    /// Read FCS UDP packets sent by the reflector.
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
                        IPEndPoint remoteEp = new (IPAddress.Any, _cfg.ReflectorPort);
                        packet = _udpClient!.Receive(ref remoteEp); // TODO: use lock?
                        _packetRecorder?.WritePacket(packet);
                    }

                    packetCount++;
                    ProcessRcvdPacket(packet);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut) {}
                //logger.Debug($"_isRunning = {_isRunning}");
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
    /// Processes incoming speech packets on a timer interval (~19ms).
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

        if (++_rxInactivityTicks > 100)
        {
            _rxInactivityTicks = 0;
            _sessionCtx.StreamState = StreamState.Lost;
        }

        if (_sessionCtx.TransceiveMode is TransceiveMode.Rx && !_rxQueue.IsEmpty)
        {
            if (_rxQueue.TryDequeue(out var pcm)) // PCM data already validated
            {
                AmbeHelper.SwapPcmBytes(pcm!);
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
    private void SendDatagram(byte[] dgram , int len)
    {
        lock (_lock)
        {
            _udpClient?.Send(dgram, len);
        }
    }

    /// <summary>
    /// Sends a formatted PING message over the network to link/login to reflector.
    /// Method is used for initial and keep-alive login.
    /// </summary>
    public void SendPing()
    {        
        StringBuilder sb = new();
        sb.Append("PING").Append(_cfg.Callsign.PadRight(6)).Append(_cfg.ReflectorId);
        string cmd = sb.ToString();
        byte[] dgram = new byte[25];
        Encoding.ASCII.GetBytes(cmd).CopyTo(dgram, 0);        
        SendDatagram(dgram, dgram.Length);

        _sessionCtx.TxPingCount++;
        if (_sessionCtx.TxPingCount % 100 == 0)
            logger.Debug($"{_sessionCtx.TxPingCount} PING packets send");
    }

    /// <summary>
    /// Sends a formatted CLOSE message over the network to unlink/logout from reflector.
    /// </summary>
    private void SendClose()
    {
        logger.Debug("");
        string cmd = "CLOSE".PadRight(11);
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);
    }

    /// <summary>
    /// Sends a formatted INFO identification message (100 bytes) over the network.
    /// </summary>
    private void SendFcsi()
    {
        StringBuilder sb = new();
        sb.Append(_cfg.RxFrequency.ToString("D9"));
        sb.Append(_cfg.TxFrequency.ToString("D9"));
        sb.Append(Helpers.FormatField(_cfg.Locator, 6));
        sb.Append(Helpers.FormatField(_cfg.HotspotType, 12));
        sb.Append(Helpers.FormatField("1234567", 7)); // TODO: fix this
        string cmd = sb.ToString().PadRight(100);
        logger.Debug($"{cmd}");
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);
    }

    /// <summary>
    /// Sends a formatted INFO identification message (80 bytes) over the network.
    /// </summary>
    private void SendFcsi2()
    {
        StringBuilder sb = new();
        sb.Append("FCSI");
        sb.Append(_cfg.ReflectorId);
        sb.Append(_cfg.RxFrequency.ToString("D9"));
        sb.Append(_cfg.TxFrequency.ToString("D9"));
        sb.Append(Helpers.FormatField(_cfg.Locator, 6));
        sb.Append(Helpers.FormatField(_cfg.Town, 20));
        sb.Append(Helpers.FormatField(_cfg.HotspotType, 12));
        sb.Append(Helpers.FormatField("1234567", 7)); // TODO: fix this
        string cmd = sb.ToString().PadRight(80);
        logger.Debug($"{cmd}");
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);
    }

    /// <summary>
    /// Sends a formatted FCSO option message over the network,
    /// containing the reflector's id and an optional list of DG-IDs.
    /// </summary>
    /// <param name="dgidList">
    /// An optional list of DG-IDs to send as options. The list is serialized as a semicolon-separated string (ending with a semicolon),
    /// and the total encoded string must not exceed 36 characters.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the DGID list string exceeds the allowed max. length of 38 bytes.
    /// </exception>
    /// <remarks>
    /// The resulting message is 50 bytes long:
    /// "FCSO" (4 bytes) + reflector id (8 bytes) + padded DGID string (max. 38 bytes).
    /// </remarks>

    private void SendFcso(List<int>? dgidList)
    {
        string dgidStr = "";
        if (dgidList != null && dgidList.Count > 0)
        {
            dgidStr = string.Join(";", dgidList) + ";";
            if (dgidStr.Length > 38) // 50 - 12
                throw new ArgumentException("Too many DGIDs; exceeds maximum allowed length.");
        }

        StringBuilder sb = new();
        sb.Append("FCSO").Append(_cfg.ReflectorId).Append(dgidStr.PadRight(38));
        string cmd = sb.ToString();
        logger.Debug($"cmd = {cmd}");
        SendDatagram(Encoding.ASCII.GetBytes(cmd), cmd.Length);
    }

    /// <summary>
    /// Starts the FCS client and initializes all required resources.
    /// </summary>
    public void StartClient()
    {
        logger.Debug("");
        if (_isRunning)
        {
            logger.Error("This FcsClient is already running.");
            return;
        }

        if (_cfg.SimulationMode && !string.IsNullOrEmpty(_cfg.SimulationFile))
        {
            logger.Debug($"Simulation mode, input file: {_cfg.SimulationFile}");
            _packetReader = new PacketRecorder(_cfg.SimulationFile, FileMode.Open, FileAccess.Read);
            _cfg.RecordFcsPackets = false; // we never record test data
        }
        else
        {
            // Create the UDP socket used by this FCS client
            logger.Debug($"Using Reflector: {_cfg.ReflectorAddress}:{_cfg.ReflectorPort}, reflectorId = {_cfg.ReflectorId}");
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
            _wavRecorder = new(_cfg.RecordAudioFile);
        }

        if (_cfg.RecordFcsPackets && !string.IsNullOrEmpty(_cfg.RecordFcsPacketsFile))
        {
            _packetRecorder = new PacketRecorder(_cfg.RecordFcsPacketsFile, FileMode.Create, FileAccess.Write);
        }

        // Thread to read FCS UDP packets
        _udpReaderThread = new Thread(ReadUdpPackets) { IsBackground = true };
        _isRunning = true;
        _udpReaderThread.Start();

        if (!_cfg.SimulationMode)
        {
            SendPing();
            SendFcsi();
            SendFcsi2();
            SendFcso(null); // TODO: fix this?
            _pingTimer.Start();
        }
    }

    /// <summary>
    /// Stops the FCS client and releases all associated resources.
    /// </summary>
    public void StopClient()
    {
        logger.Debug($"_isRunning = {_isRunning}");
        SendClose();
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
            _sessionCtx.TransceiveMode = TransceiveMode.Tx;
            _sessionCtx.TxFrameNumber = 0;
            byte[] packet = FusionCodec.Encoder.EncodeHeader(false, false, _sessionCtx.TxFrameNumber, _cfg.Callsign, reflName: _cfg.ReflectorId);
            SendDatagram(packet, 130);
            _sessionCtx.TxFrameNumber++;
            logger.Debug($"sent HC: {_sessionCtx.TxFrameNumber}");

            _cfg.MicrophoneReader?.Start();
            _txTimer.Start();
        }
        else
        {
            _cfg.MicrophoneReader?.Stop();
            
            byte[] packet = FusionCodec.Encoder.EncodeHeader(false, true, _sessionCtx.TxFrameNumber, _cfg.Callsign, reflName: _cfg.ReflectorId);
            SendDatagram(packet, 130);
            _sessionCtx.TxFrameNumber++;
            logger.Debug($"sent TC: {_sessionCtx.TxFrameNumber}");

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
            if (_cfg.MicrophoneReader != null && _cfg.MicrophoneReader.TryRead(ambeSpeechPacket, 6, ambeSpeechPacket.Length - 6, out int bytesRead))
            {
                AmbeHelper.SwapPcmBytes(ambeSpeechPacket); // LE -> BE
                _ambeClient.SendPacket(ambeSpeechPacket);
                byte[] ambe = _ambeClient.ReceivePacket();
                if (AmbeHelper.IsAmbePacket(ambe))
                    _txQueue.Enqueue(ambe[6..]); // ignore 6 byte header
            }

            // TODO: consider VW mode
            if (_txQueue.Count >= 5)
            {
                _sessionCtx.TxAmbeData.Clear();
                for (int i = 0; i < 5; i++)
                {
                    if (_txQueue.TryDequeue(out byte[]? item) && item.Length == 7)
                    {
                        _sessionCtx.TxAmbeData.Add(item);
                    }
                    else
                    {
                        logger.Error($"Unable to read TX queue");
                        // TODO: throw exception?
                    }
                }
                if (_sessionCtx.TransceiveMode == TransceiveMode.Tx) // late check because of thread gym
                {
                    byte[] packet = FusionCodec.Encoder.EncodeVD2(false, _sessionCtx.TxFrameNumber, _sessionCtx.TxAmbeData, _cfg.Callsign, reflName: _cfg.ReflectorId);
                    SendDatagram(packet, 130);
                    _sessionCtx.TxFrameNumber++;
                    logger.Debug($"sent CC: {_sessionCtx.TxFrameNumber}");
                }
            }
        }
    }
}