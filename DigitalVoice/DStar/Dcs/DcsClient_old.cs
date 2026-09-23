
using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
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
public class DcsClient_old
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    readonly object _lock = new();

    readonly DcsClientConfig _cfg;

    readonly DcsClientState _clientState;

    readonly int _socketTimeout = 1000; // ms

    UdpClient? _udpClient;

    Thread? _dcsPacketReaderThread; // read DCS UDP packets

    readonly System.Timers.Timer _pingTimer = new(2000);

    readonly System.Timers.Timer _rxTimer = new(20);

    readonly System.Timers.Timer _txTimer = new(20);

    readonly ConcurrentQueue<byte[]> _rxQueue = new();

    PacketRecorder? _packetRecorder; // used for read/write operations, only one at a time

    int _rxInactivityTicks = 0;

    // DMR data Consumer delegate
    public delegate void ConsumeDcsData(DcsClientState state, bool clearFields);

    public ConsumeDcsData? ExternalDcsDataConsumer;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cfg"></param>
    public DcsClient_old(DcsClientConfig cfg)
    {
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

    /// <summary>
    /// Send the argument bytes to the DCS reflector. 
    /// </summary>
    /// <param name="dgram">The bytes to send.</param>
    /// <param name="len">The lengthof the datagram to sent.</param>
    private void SendDatagram(byte[] dgram, int len)
    {
        lock (_lock)
        {   // UdpClient is not thread safe
            _udpClient?.Send(dgram, len);
        }
    }

    readonly byte[] _userData = new byte[21];
    

    private void ProcessRcvdDcsPacket(byte[] packet)
    {
        //logger.Debug($"({packet.Length}) - {Convert.ToHexString(packet)}");

        switch (packet.Length)
        {
            case 9:
                return;
            case 14:
                if (Encoding.ASCII.GetString(packet, 10, 3) == "ACK")
                {
                    if (!_cfg.SimulationMode) 
                        _pingTimer.Start();
                }
                return;
            case 22:
                // Ping reply
                _clientState.RxPingCount++;
                if (_clientState.RxStreamState is StreamState.Lost or StreamState.End)
                {
                    _clientState.RxStreamState = StreamState.Idle;
                }
                return;
            case 35:
                int idx = Array.IndexOf(packet, (byte)0x00, 0);
                string msg = Encoding.ASCII.GetString(packet, 0, idx);
                logger.Debug($"net message = {msg}");
                return;
            case 100:
                if (Encoding.ASCII.GetString(packet, 0, 4) != "0001")
                {
                    logger.Error($"Unexpected packet: len {packet.Length} - {Convert.ToHexString(packet)}");
                    return;
                }
                break; // continue processing packet
            default:
                logger.Warn($"Unexpected packet: len {packet.Length} - {Convert.ToHexString(packet)}");
                return;
        }

        _rxInactivityTicks = 0;

        if (_clientState.TransceiveMode == TransceiveMode.Rx && _clientState.RxStreamId == 0)
        {
            _clientState.RxStreamId = (ushort)(packet[43] << 8 | packet[44]);
            _clientState.RxStreamState = StreamState.New;
            //_modeInfo.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (!_rxTimer.Enabled)
            {
                //_audio.StartPlayback();
                _rxTimer.Start();
                _rxQueue.Clear();
            }

            //_clientState.Gw2 = Encoding.ASCII.GetString(packet, 7, 8).TrimEnd();
            //_clientState.Gw = Encoding.ASCII.GetString(packet, 15, 8).TrimEnd();
            //_clientState.Dst = Encoding.ASCII.GetString(packet, 23, 8).TrimEnd();
            //_clientState.Src = Encoding.ASCII.GetString(packet, 31, 8).TrimEnd();

            // Decode the whole 32-byte block once
            string headerBlock = Encoding.ASCII.GetString(packet, 7, 32);

            // Extract fields (8 bytes each), trimming trailing spaces
            _clientState.RxRptr2 = headerBlock[..8].TrimEnd();
            _clientState.RxRptr1 = headerBlock.Substring(8, 8).TrimEnd();
            _clientState.RxUrCall = headerBlock.Substring(16, 8).TrimEnd();
            _clientState.RxSrc = headerBlock.Substring(24, 8).TrimEnd();

            logger.Debug($"New stream from {_clientState.RxSrc} to {_clientState.RxUrCall}, id=0x{_clientState.RxStreamId:X}");
        }
        else
        {
            _clientState.RxStreamState = StreamState.Streaming;
        }

        _clientState.FrameNo = packet[0x2D];

        // User data extraction logic
        if (packet[45] == 0 && packet[55] == 0x55 && packet[56] == 0x2D && packet[57] == 0x16)
        {
            _clientState.SdSync = true;
            _clientState.SdSeq = 1;
        }
        else
        {
            _clientState.SdSync = false;
        }

        if (_clientState.SdSync)
        {
            switch (_clientState.SdSeq)
            {
                case 1 when packet[45] == 1 && packet[55] == 0x30:
                    _userData[0] = (byte)(packet[56] ^ 0x4F);
                    _userData[1] = (byte)(packet[57] ^ 0x93);
                    _clientState.SdSeq++;
                    break;
                case 2 when packet[45] == 2:
                    _userData[2] = (byte)(packet[55] ^ 0x70);
                    _userData[3] = (byte)(packet[56] ^ 0x4F);
                    _userData[4] = (byte)(packet[57] ^ 0x93);
                    _clientState.SdSeq++;
                    break;
                case 3 when packet[45] == 3 && packet[55] == 0x31:
                    _userData[5] = (byte)(packet[56] ^ 0x4F);
                    _userData[6] = (byte)(packet[57] ^ 0x93);
                    _clientState.SdSeq++;
                    break;
                case 4 when packet[45] == 4:
                    _userData[7] = (byte)(packet[55] ^ 0x70);
                    _userData[8] = (byte)(packet[56] ^ 0x4F);
                    _userData[9] = (byte)(packet[57] ^ 0x93);
                    _clientState.SdSeq++;
                    break;
                case 5 when packet[45] == 5 && packet[55] == 0x32:
                    _userData[10] = (byte)(packet[56] ^ 0x4F);
                    _userData[11] = (byte)(packet[57] ^ 0x93);
                    _clientState.SdSeq++;
                    break;
                case 6 when packet[45] == 6:
                    _userData[12] = (byte)(packet[55] ^ 0x70);
                    _userData[13] = (byte)(packet[56] ^ 0x4F);
                    _userData[14] = (byte)(packet[57] ^ 0x93);
                    _clientState.SdSeq++;
                    break;
                case 7 when packet[45] == 7 && packet[55] == 0x33:
                    _userData[15] = (byte)(packet[56] ^ 0x4F);
                    _userData[16] = (byte)(packet[57] ^ 0x93);
                    _clientState.SdSeq++;
                    break;
                case 8 when packet[45] == 8:
                    _userData[17] = (byte)(packet[55] ^ 0x70);
                    _userData[18] = (byte)(packet[56] ^ 0x4F);
                    _userData[19] = (byte)(packet[57] ^ 0x93);
                    _userData[20] = 0;
                    _clientState.RxUsrMsg = Encoding.ASCII.GetString(_userData).TrimEnd('\0');
                    _clientState.SdSync = false;
                    _clientState.SdSeq = 0;
                    break;
            }
        }

        // Stream end
        if ((packet[45] & 0x40) != 0)
        {
            logger.Debug("DCS RX stream ended");
            _rxInactivityTicks = 0;
            _clientState.RxStreamState = StreamState.End;
            //_clientState.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _clientState.RxStreamId = 0;
        }
        else if (_clientState.RxStreamState == StreamState.Streaming)
        {
            // TODO: ???
        }

        ExternalDcsDataConsumer?.Invoke(_clientState, false);

        logger.Trace($"{_clientState}");

        _rxQueue.Enqueue(packet[46..55]); // AMBE data

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

        if (_clientState.TransceiveMode is TransceiveMode.Rx && !_rxQueue.IsEmpty)
        {
            if (_rxQueue.TryDequeue(out var ambeData))
            {
                if (ambeData.Length != 9)
                    throw new ArgumentException($"Invalid AMBE data: {Convert.ToHexString(ambeData)}");

                Buffer.BlockCopy(ambeData, 0, ambeChannelPacket, 6, 9);
                _cfg.AmbeController?.SendPacket(ambeChannelPacket);
                byte[]? resp = _cfg.AmbeController!.ReceivePacket();
                //byte[]? resp = cfg.AmbeController!.SendReceivePacket(ambeChannelPacket);
                if (AmbeHelper.IsSpeechPacket(resp))
                {
                    AmbeHelper.SwapPcmBytes(resp!);
                    _cfg.AudioPlayer?.FeedPcmData(resp!, 6, resp!.Length - 6);
                    // wav recorder here!
                }
                else
                {
                    logger.Error("Unexpected response from AMBE server: {0}", resp != null ? Convert.ToHexString(resp) : "null");
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
        //sw.Stop();
        //logger.Debug($"{sw.ElapsedMilliseconds}");
    }

    /// <summary>
    /// Send a connect message to the DCS reflector.
    /// </summary>
    public void SendConnect()
    {
        logger.Debug("");

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
        logger.Debug("");

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

        _cfg.AmbeController!.Open();
        
        InitDV3000();

        if (_cfg.SimulationMode && _cfg.RecordRcvdUdpPackets)
        {
            _cfg.RecordRcvdUdpPackets = false; // never record test data
            logger.Warn($"Set {nameof(_cfg.RecordRcvdUdpPackets)}={_cfg.RecordRcvdUdpPackets} because test data are never recorded.");
        }

        string dataDir = "data";
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        // Setup packet recorder if need
        if (_cfg.RecordRcvdUdpPackets)
        {
            string packetFile = Path.Combine("data", $"{GetType().Name}_packets_{DateTime.Now:yyyyMMddHHmmss}.bin");
            _packetRecorder = new PacketRecorder(packetFile, FileMode.Create, FileAccess.Write);
        }

        // Setup for simulation mode if needed
        if (_cfg.SimulationMode)
        {
            ArgumentException.ThrowIfNullOrEmpty(_cfg.SimulationFile, nameof(_cfg.SimulationFile));
            _packetRecorder = new PacketRecorder(_cfg.SimulationFile, FileMode.Open, FileAccess.Read);
            logger.Debug($"Simulation mode, input file: {_cfg.SimulationFile}");
        }

        _clientState.RxPingCount = 0;
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

        _cfg.AmbeController!.Close();
    }
}