using AmbeServer;
using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using DigitalVoice.DStar.Common;
using NLog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace DigitalVoice.DStar.Dcs;

/// <summary>
/// Implementation of a Digital Call Server (DCS) client.
/// </summary>
public class DcsClient
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    readonly object _lock = new();

    readonly DcsClientConfig _cfg;

    readonly DStarSessionContext _clientState;

    readonly int _socketTimeout = 1000; // ms

    UdpClient? _udpClient;

    Thread? _dcsPacketReaderThread; // read DCS UDP packets

    readonly System.Timers.Timer _pingTimer = new(5000);

    readonly System.Timers.Timer _rxTimer = new(100); // 5 AMBE blocks

    readonly System.Timers.Timer _txTimer = new(20);

    readonly AmbeClient _ambeClient;

    readonly ConcurrentQueue<byte[]> _rxQueue = new();

    readonly ConcurrentQueue<byte[]> _txQueue = new();

    PacketRecorder? _packetRecorder; // used for read/write operations, only one at a time

    WavPcmRecorder? _wavRecorder;

    int _rxInactivityTicks = 0;

    // DCS data Consumer delegate
    public delegate void ConsumeDcsData(DStarSessionContext state);
    public delegate void ConsumeNetMsg(string msg);

    public ConsumeDcsData? ExternalDcsDataConsumer;
    public ConsumeNetMsg? ExternalNetMsgConsumer;


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cfg"></param>
    public DcsClient(DcsClientConfig cfg)
    {
        _cfg = cfg;

        _clientState = new() { Reflector = _cfg.RefName + _cfg.Module };

        _pingTimer.Elapsed += PingTimerCallback;
        _rxTimer.Elapsed += RxTimerCallback;
        _txTimer.Elapsed += TxTimerCallback;

        _ambeClient = new();

        if (_cfg.SimulationMode)
        {
            //throw new NotImplementedException("Simulation mode not implemented yet.");
        }
        else
        {
            Connect();
        }

        InitDV3000();
    }

    /// <summary>
    /// Initializes the DV3000 device by sending a predefined sequence of control packets,
    /// including configuration query, software reset, encoder mode setup, and custom rate selection.
    /// </summary>
    private void InitDV3000()
    {
        if (_ambeClient == null) return;
        byte[][] packets =
        [
            [0x61, 0x00, 0x01, 0x00, 0x36], // Query for configuration pin state at power-up or reset
            [0x61, 0x00, 0x07, 0x00, 0x34, 0x05, 0x00, 0x00, 0x07, 0x00, 0x10], // Reset the device with software configuration
            [0x61, 0x00, 0x03, 0x00, 0x05, 0x10, 0x40], // Encoder cmode flags for current channel
            [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x01, 0x30, 0x07, 0x63, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48] // custom rate, interoperable with D-Star as stated by DVSI
        ];

        foreach (var p in packets)
        {
            byte[] resp = _ambeClient.SendReceivePacket(p);
            logger.Debug($"send: {Convert.ToHexString(p)}");
            logger.Debug($"rcvd: {Convert.ToHexString(resp)}");
        }
    }

    /// <summary>
    /// Establishes a UDP connection to the configured DCS reflector address and port.
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

    internal bool CaptureAudio()
    {
        // build header for 160 sample audio packet
        int payloadSize = 160 * 2;        // 320 bytes
        int totalSize = 6 + payloadSize;
        int lengthField = totalSize - 4;   // = 322 -> 0x0142

        var pcm = new byte[totalSize];
        pcm[0] = 0x61;
        pcm[1] = (byte)(lengthField >> 8);
        pcm[2] = (byte)(lengthField & 0xFF);
        pcm[3] = 0x02;
        pcm[4] = 0x00;
        pcm[5] = 0xA0; // 160 PCM samples

        if (_cfg.MicrophoneReader == null || !_cfg.MicrophoneReader.TryRead(pcm, 6, pcm.Length - 6, out int bytesRead))
            return false;

        // TODO: swap pcm bytes
        AmbeHelper.SwapPcmBytes(pcm); // TODO: common method
        _ambeClient.SendPacket(pcm);
        byte[] ambe = _ambeClient.ReceivePacket();
        if (ambe[0] == 0x61 && ambe[3] == 0x01 && ambe[4] == 0x01)
        {
            _txQueue.Enqueue(ambe[6..]);
        }
        else
        {
            logger.Error($"Unexpected response of AMBE server: {Convert.ToHexString(ambe)}");
            return false;
        }
        return true;
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
    /// Call back method to login the DCS reflector (keep alive). 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PingTimerCallback(object? sender, ElapsedEventArgs e)
    {
        SendPing();
    }

    private void ProcessRcvdDcsPacket(byte[] packet)
    {
        //logger.Debug($"({packet.Length}) - {Convert.ToHexString(packet)}");
        switch (packet.Length)
        {
            case 9:
                break;
            case 14:
                logger.Debug($"({packet.Length}) - {Convert.ToHexString(packet)}");
                if (Encoding.ASCII.GetString(packet, 10, 3) == "ACK")
                {
                    _clientState.RxStreamId = 0;
                    if (!_cfg.SimulationMode)
                        _pingTimer.Start();
                }
                break;
            case 22:
                // Ping reply from server
                _clientState.RxPingCnt++;
                //logger.Debug($"Pings received: {_clientState.RxPingCnt}");
                if (_clientState.RxPingCnt % 100 == 0) 
                    logger.Debug($"Number of pings rcvd: {_clientState.RxPingCnt}");

                if (_clientState.RxStreamState is StreamState.Lost or StreamState.End)
                {
                    _clientState.RxStreamState = StreamState.Idle;
                }
                break;
            case 35:
                int idx = Array.IndexOf(packet, (byte)0x00, 0);
                string msg = Encoding.ASCII.GetString(packet, 0, idx);
                logger.Debug($"Net message = {msg}");
                if (!string.IsNullOrEmpty(msg))
                {
                    _clientState.RxNetMsg = msg;
                    ExternalNetMsgConsumer?.Invoke(_clientState.RxNetMsg);
                }
                break;
            case 100:
                if (Encoding.ASCII.GetString(packet, 0, 4) == "0001")
                {
                    _rxInactivityTicks = 0;
                    byte[] ambeData = DcsCodec.DecodeDcs100Frame(packet, _clientState);
                    if (_clientState.RxStreamState is StreamState.New && !_rxTimer.Enabled)
                    {
                        _rxQueue.Clear();
                        _rxTimer.Start();
                    }
                    _rxQueue.Enqueue(ambeData);

                    //logger.Trace($"{_clientState}");
                    ExternalDcsDataConsumer?.Invoke(_clientState);
                }
                else
                {
                    logger.Error($"Unexpected packet: len {packet.Length} - {Convert.ToHexString(packet)}");
                }
                break;
            default:
                logger.Warn($"Unexpected packet: len {packet.Length} - {Convert.ToHexString(packet)}");
                break;
        }
    }


    /// <summary>
    /// Continuously reads DCS UDP packets from the network and dispatches them for processing.
    /// </summary>
    /// <remarks>
    /// This method runs in a loop while the <c>_isRunning</c> flag is <c>true</c>. It receives UDP datagrams from the configured 
    /// UDP client, logs the received data in hexadecimal format, and forwards each packet to <see cref="ProcessRcvdDcsPacket"/> 
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
                        if (_cfg.RecordRcvdUdpPackets)
                            _packetRecorder?.WritePacket(packet);
                    }

                    packetCount++;
                    ProcessRcvdDcsPacket(packet);
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

            _packetRecorder?.Close();
            _packetRecorder = null;

            _wavRecorder?.Close();
            _wavRecorder = null;
        }
    }

    static readonly byte[] ambeChannelPacket = [0x61, 0x00, 0x0B, 0x01, 0x01, 0x48, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]; // 15 bytes 

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

        if (_rxInactivityTicks++ > 100)
        {
            _rxInactivityTicks = 0;
            _clientState.RxStreamId = 0;
            _clientState.RxStreamState = StreamState.Lost;
            ExternalDcsDataConsumer?.Invoke(_clientState);
        }

        int n = 5; // <-- this must match the rxTimer period (5 -> 100ms) 
        if (_clientState.TransceiveMode is TransceiveMode.Rx && _rxQueue.Count >= n)
        {
            for (int i = 0; i < n; i++)
            {
                if (_rxQueue.TryDequeue(out byte[]? ambeData))
                {
                    if (ambeData.Length != 9)
                        throw new ArgumentException($"Invalid AMBE data len: {Convert.ToHexString(ambeData)}");

                    ambeData.CopyTo(ambeChannelPacket, 6);
                    _ambeClient?.SendPacket(ambeChannelPacket);
                }
                else
                {
                    logger.Error("Unable to read rx queue");
                }
            }

            for (int i = 0; i < n; i++)
            {
                byte[]? pcmData = _ambeClient?.ReceivePacket();
                if (AmbeHelper.IsSpeechPacket(pcmData))
                {
                    AmbeHelper.SwapPcmBytes(pcmData!);
                    _cfg.AudioPlayer?.FeedPcmData(pcmData!, 6, pcmData!.Length - 6);
                    _wavRecorder?.WritePcm(pcmData!, 6, pcmData!.Length - 6);
                }
                else
                {
                    logger.Error("Unexpected response from AMBE server: {0}", pcmData != null ? Convert.ToHexString(pcmData) : "null");
                }
            }
        }
        else if (_clientState.RxStreamState is StreamState.Lost or StreamState.End)
        {
            _rxInactivityTicks = 0;
            _rxTimer.Stop();
            _clientState.RxStreamId = 0;
            _clientState.RxStreamState = StreamState.Idle;
            //logger.Debug("RX queue empty");
        }
        //sw.Stop();
        //logger.Debug($"{sw.ElapsedMilliseconds}");
    }

    /// <summary>
    /// Send a connect message to the DCS reflector.
    /// </summary>
    public void SendConnect()
    {
        logger.Debug($"Callsign={_cfg.Callsign}, Reflector={_cfg.RefName}, Module={_cfg.Module}");

        byte[] outBuffer = new byte[519];

        // Callsign padded to 8 bytes
        var callsignBytes = Encoding.ASCII.GetBytes(_cfg.Callsign);
        Array.Copy(callsignBytes, 0, outBuffer, 0, callsignBytes.Length);
        for (int i = callsignBytes.Length; i < 8; i++)
            outBuffer[i] = (byte)' ';

        // Module info
        outBuffer[8] = (byte)_cfg.Module;
        outBuffer[9] = (byte)_cfg.Module;
        outBuffer[10] = 11;

        SendDatagram(outBuffer, outBuffer.Length);
    }

    /// <summary>
    /// Send the argument bytes to the DCS reflector. 
    /// </summary>
    /// <param name="dgram">The bytes to send.</param>
    /// <param name="len">The length of the datagram to sent.</param>
    private void SendDatagram(byte[] dgram, int len)
    {
        lock (_lock)
        {   // UdpClient is not thread safe
            _udpClient?.Send(dgram, len);
        }
    }

    /// <summary>
    /// Send a disconnect message to the DCS reflector.
    /// </summary>
    public void SendDisconnect()
    {
        logger.Debug("");

        // Callsign padded to 8 chars
        string paddedCallsign = _cfg.Callsign.PadRight(8, ' ');
        var callsignBytes = Encoding.ASCII.GetBytes(paddedCallsign);

        // Allocate exact size: 8 + 1 + 1 + 1 = 11
        var buffer = new byte[11];

        // Copy callsign
        Buffer.BlockCopy(callsignBytes, 0, buffer, 0, 8);

        // Add module
        buffer[8] = (byte)_cfg.Module;

        // Add space
        buffer[9] = (byte)' ';

        // Null terminator
        buffer[10] = 0x00;

        SendDatagram(buffer, buffer.Length);
    }

    /// <summary>
    /// Send a ping message to the DCS reflector.
    /// </summary>
    public void SendPing()
    {
        //logger.Debug("");

        // Callsign padded to 7 chars
        string paddedCallsign = _cfg.Callsign.PadRight(7, ' ');
        var callsignBytes = Encoding.ASCII.GetBytes(paddedCallsign);
        var refNameBytes = Encoding.ASCII.GetBytes(_cfg.RefName);

        // Total length = 7 (Callsign) + 1 (Module) + 1 (null) + RefName.Length + 1 (null) + 1 (module)
        int totalLength = 7 + 1 + 1 + refNameBytes.Length + 1 + 1;
        var buffer = new byte[totalLength];

        int offset = 0;

        // Copy callsign
        Buffer.BlockCopy(callsignBytes, 0, buffer, offset, 7);
        offset += 7;

        // Module
        buffer[offset++] = (byte)_cfg.Module;

        // Null terminator
        buffer[offset++] = 0x00;

        // RefName
        Buffer.BlockCopy(refNameBytes, 0, buffer, offset, refNameBytes.Length);
        offset += refNameBytes.Length;

        // Null terminator
        buffer[offset++] = 0x00;

        // Module again
        buffer[offset++] = (byte)_cfg.Module;

        SendDatagram(buffer, buffer.Length);

        _clientState.TxPingCnt++;
        if (_clientState.TxPingCnt % 60 == 0) // every 5 min
            logger.Debug($"Number of pings sent: {_clientState.TxPingCnt}");
    }

    /// <summary>
    /// Starts this this DCS client.
    /// </summary>
    public void Start()
    {
        logger.Debug($"{_clientState.IsRunning}");
        if (_clientState.IsRunning)
        {
            logger.Error($"Client is already running.");
            return;
        }

        if (_cfg.SimulationMode && _cfg.RecordRcvdUdpPackets)
        {
            _cfg.RecordRcvdUdpPackets = false; // never record test data
            logger.Warn($"Set {nameof(_cfg.RecordRcvdUdpPackets)}={_cfg.RecordRcvdUdpPackets}, reason: simulation mode is active.");
        }

        string dataDir = "data";
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        // Setup Audio recorder if need
        if (_cfg.RecordAudio)
        {
            string wavFile = Path.Combine("data", $"dcs_audio_{DateTime.Now:yyyyMMddHHmmss}.wav");
            _wavRecorder = new(wavFile);
        }

        // Setup packet recorder if need
        if (_cfg.RecordRcvdUdpPackets)
        {
            string filePath = string.IsNullOrEmpty(_cfg.RecordRcvdUdpPacketsFile) ? Path.Combine("data", $"dcs_udp_packets_{DateTime.Now:yyyyMMddHHmmss}.bin") : _cfg.RecordRcvdUdpPacketsFile;
            _packetRecorder = new PacketRecorder(filePath, FileMode.Create, FileAccess.Write);
        }

        // Setup for simulation mode if needed
        if (_cfg.SimulationMode)
        {
            ArgumentException.ThrowIfNullOrEmpty(_cfg.SimulationFile, nameof(_cfg.SimulationFile));
            _packetRecorder = new PacketRecorder(_cfg.SimulationFile, FileMode.Open, FileAccess.Read);
            logger.Debug($"Simulation mode, input file: {_cfg.SimulationFile}");
        }

        _clientState.RxPingCnt = 0;
        _clientState.RxStreamId = 0;
        _clientState.IsRunning = true;
        _dcsPacketReaderThread = new Thread(ReadUdpPackets) { IsBackground = true };
        _dcsPacketReaderThread.Start();
        SendConnect();
    }

    /// <summary>
    /// Stops this DCS client.
    /// </summary>
    public void Stop()
    {
        logger.Debug($"{_clientState.IsRunning}");
        _clientState.IsRunning = false;
        _pingTimer.Stop();
        SendDisconnect(); // TODO: check cmd sequence
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
            
            _clientState.TransceiveMode = TransceiveMode.Tx;
            
            _rxTimer.Stop();
            _rxQueue.Clear();
            // TODO: make this sane!
            _clientState.TxRptr1 = _cfg.Callsign.PadRight(7, ' ') + 'C';
            _clientState.TxRptr2 = _cfg.RefName.PadRight(7, ' ') + _cfg.Module;
            _clientState.TxMyCall = _cfg.Callsign;
            _clientState.TxUrCall = "CQCQCQ";

            _clientState.TxUsrMsg = _cfg.UserMessage.PadRight(20, ' ');

            _clientState.TxFrameCnt = 0;
           
            _cfg.MicrophoneReader?.Start();
            _txTimer.Start();
        }
        else
        {
            _txTimer.Stop();
            _txQueue.Clear();

            byte[] dcs100Frame = DcsCodec.CreateDcs100Frame(true, _clientState, new byte[9]);
            SendDatagram(dcs100Frame, dcs100Frame.Length);
            _clientState.TxFrameCnt++;

            //_clientState.TxFrameCnt = 0;
            //_clientState.TxStreamId = 0;

            _clientState.TransceiveMode = TransceiveMode.Rx;
            _cfg.MicrophoneReader?.Stop();
        }
    }

    internal void TxTimerCallback(object? sender, ElapsedEventArgs e)
    {
        //logger.Debug($"_clientState = {_clientState}");
        if (_clientState.TransceiveMode == TransceiveMode.Tx)
        {
            _clientState.RxStreamState = StreamState.Transmitting; // TODO: nötig???

            if (!CaptureAudio()) // TODO: sollte in den if-Branch?
                return;

            if (!_txQueue.IsEmpty)
            {
                if (_txQueue.TryDequeue(out byte[]? ambeData) && ambeData.Length == 9)
                {
                    byte[] dcs100Frame = DcsCodec.CreateDcs100Frame(false, _clientState, ambeData);
                    SendDatagram(dcs100Frame, dcs100Frame.Length);
                    _clientState.TxFrameCnt++;
                }
                else
                {
                    logger.Error("Unable to read TX queue");
                    // TODO: this should never happen, what to do?
                }
            }
        }
        
        ExternalDcsDataConsumer?.Invoke(_clientState);
    }
}