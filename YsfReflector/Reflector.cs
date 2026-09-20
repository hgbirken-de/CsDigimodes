using FusionCodec;
using NLog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Timers;

namespace YsfReflector;

/// <summary>
/// YSF Reflector implementation.
/// </summary>
public class Reflector
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly string GW_DGID_MAP_FILE = "./db/gw_dgid_map.json"; // TODO: -> config

    static readonly byte[] FrameSync = [0xD4, 0x71, 0xC9, 0x63, 0x4D];

    static readonly Random random = new();

    readonly Config _cfg;

    readonly ConcurrentDictionary<IPEndPoint, Client> _clientMap = [];

    bool _isRunning = false;

    readonly BlockingCollection<(IPEndPoint, byte[])> _rcvdPackets = []; // buffer for rcvd UDP packets

    UdpClient? _udpClient;

    Thread? _udpReaderThread;
    Thread? _procRcvdPacketThread;
     
    readonly object _lock = new();

    readonly System.Timers.Timer _checkClientInactivityTimer;

    readonly System.Timers.Timer _checkStreamInactivityTimer;

    private readonly AccessControl _accessControl;

    private readonly HomeDgIds _homeDgIds;

    private int _nextGwId = 1; // must be positive

    private Dictionary<string, int> _gwDgIdMap = [];

    private readonly List<YsfdStream> _activeStreams = []; // active streams
    private string _dstCallsign = string.Empty;
    private readonly List<int> _rxLock = [];
    private readonly Dictionary<int, float> _rxLockTout = [];


    /// <summary>
    /// Constructor.
    /// </summary>
    public Reflector()
    {
        _cfg = Config.Instance();

        _accessControl = new(_cfg.ClientMgmt.AccessControlFile, _clientMap);

        //IPEndPoint endpoint = new(IPAddress.Parse("127.0.0.1"), 42000);
        //bool b = _accessControl.IsIpBlocked(endpoint);

        _homeDgIds = new(_cfg.DgIdMgmt.HomeDgIdFile);

        _checkClientInactivityTimer = new(_cfg.ClientMgmt.InactivityTimeoutSeconds*1000);
        _checkClientInactivityTimer.Elapsed += CheckForInactiveClients;

        _checkStreamInactivityTimer = new(100); // TODO: -> config

        logger.Debug($"version = {_cfg.Info.Version}, listening at = {_cfg.Network.IpAddr}:{_cfg.Network.Port}");
    }

    /// <summary>
    /// Timer Callback method that checks for clients which are inactive for a defined while. Such clients are removed from the internal client map.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckForInactiveClients(object? sender, ElapsedEventArgs e)
    {
        logger.Debug("");
        if (_clientMap.IsEmpty) return;
        var now = DateTime.UtcNow;
        List<Client> toRemove;
        lock (_lock)
        {
            toRemove = [.. _clientMap.Where(kvp => (now - kvp.Value.LastSeen).TotalSeconds > _cfg.ClientMgmt.MaxInactivityPeriodSeconds).Select(kvp => kvp.Value)];
            foreach (var c in toRemove)
            {
                _clientMap.TryRemove(c.EndPoint, out _);
            }
        }

        foreach (var c in toRemove)
        {
            logger.Info($"Removed inactive client: {c}");
        }
    }

    /// <summary>
    /// Normalizes a callsign by trimming whitespace and removing any suffix or modifier following a dash (<c>-</c>) or slash (<c>/</c>).
    /// </summary>
    /// <param name="callsign">The input callsign, which may include suffixes (e.g. <c>DL1ABC/N</c> or <c>JA1XYZ-10</c>).</param>
    /// <returns>The cleaned base callsign without suffix or modifiers. Returns an empty string if the input is <see langword="null"/>, empty, or whitespace.</returns>
    /// <example>
    /// <code>
    /// CleanCallsign("DL1ABC/N");   // returns "DL1ABC"
    /// CleanCallsign("JA1XYZ-10");  // returns "JA1XYZ"
    /// CleanCallsign("  M0ABC  ");  // returns "M0ABC"
    /// </code>
    /// </example>
    public static string CleanCallsign(string callsign)
    {
        if (string.IsNullOrWhiteSpace(callsign)) return string.Empty;
        callsign = callsign.Trim();
        return Regex.Split(callsign, @"[-/]")[0].Trim();
    }

    public static string DecodeDch(byte[] data) => new([.. data.Select(b => (b > 31 && b < 127) ? (char)b : ' ')]);


    /// <summary>
    /// Extracts a DG-ID number from a callsign string that may include a hyphen-separated DG-ID suffix (e.g., "DL1HGB-91").
    /// </summary>
    /// <param name="callsign">The input callsign, optionally containing a DG-ID suffix.</param>
    /// <returns>
    /// A tuple where:
    /// <list type="bullet">
    ///   <item><description>Item1 (<see cref="string"/>): The base callsign (part before the hyphen).</description></item>
    ///   <item><description>Item2 (<see cref="int"/>): The DG-ID value if successfully parsed; otherwise <c>0</c>.</description></item>
    /// </list>
    /// </returns>
    internal static (string, int) DgIdFromCallsign(string callsign)
    {
        string[] s = callsign.Split('-');
        if (s.Length == 2)
        {
            if (int.TryParse(s[1], out int value))
            {
                return (s[0], value);
            }
        }
        return (callsign, 0);
    }

    internal void HandleYsfd(IPEndPoint endPoint, byte[] packet)
    {
        if (_clientMap.TryGetValue(endPoint, out var client))
        {
            client.LastSeen = DateTime.UtcNow;
            // TODO: time home ??
        }
        else
        {
            logger.Warn($"Unknown client: endPoint={endPoint}, packet={Convert.ToHexString(packet)}");
            return;
        }

        int auxDgid = 0; // usually assigned to socket, currently a placeholder/reminder for future extension

        // Try to assert frame sync
        if (!packet.AsSpan(35, FrameSync.Length).SequenceEqual(FrameSync))
        {
            logger.Error($"Unable to assert frame sync: endPoint = {endPoint}, packet = {Convert.ToHexString(packet)}");
            // TODO: what to do in this situation?
        }

        bool dgIdBusy = false;
        string rem12 = string.Empty;
        string rem34 = string.Empty;
        int radioCode = 0;
        bool coordSend = false;

        YsfdStream? tx = null;
        foreach (var st in _activeStreams)
        {
            // Check if the DG-ID of this stream matches the gateway's DG-ID
            if (st.DgId == client.DgId)
                dgIdBusy = true;

            // Determine if we should reuse this stream
            bool reuse = (st.DgId == client.DgId && st.GwId == client.GwId) || (client.DgId == _cfg.DgIdMgmt.DgIdLocal && st.DgId == -client.GwId);
            if (reuse)
            {
                tx = st; // reuse existing stream
                rem12 = st.Rem12 ?? "";
                rem34 = st.Rem34 ?? "";
                radioCode = st.RadioCode;
                coordSend = st.CoordSend;
                break; // 1st matching stream wins
            }
        }
        tx ??= YsfdStream.FromPacket(packet); // no stream to reuse found

        if (!Fich.Decode(packet))
        {
            logger.Error("Unable to decode FICH.");
            return;
        }

        CallMode cm = (CallMode)Fich.GetCM();
        DataType dt = (DataType)Fich.GetDT();
        FrameInformation fi = (FrameInformation)Fich.GetFI();
        int ft = Fich.GetFT();
        int fn = Fich.GetFN();
        int sq = Fich.GetSC();

        byte[]? packet_mod = null;

        if (fi == FrameInformation.HC)
        {   // header channel
            // init GPS here ...
            (bool valid1, string? dst, _) = FusionCodec.Decoder.DecodeHeader(true, packet);
            if (valid1)
            {
                _dstCallsign = dst ?? ""; // TODO: -> tx stream object
                (bool valid2, string? downLink, string? upLink) = FusionCodec.Decoder.DecodeHeader(false, packet);
                if (valid2)
                {
                    if (_cfg.DgIdMgmt.Prefix > 0)
                    {
                        packet_mod = new byte[155];
                        packet.CopyTo(packet_mod, 0);

                        string csd1String = dst!.PadRight(10) + $"{client.DgId}/{tx.SourceClean}".PadRight(10);
                        string csd2String = downLink!.PadRight(10) + upLink!.PadRight(10);

                        byte[] csd1 = Encoding.ASCII.GetBytes(csd1String);
                        byte[] csd2 = Encoding.ASCII.GetBytes(csd2String);
                        FusionCodec.Encoder.WriteDchData(true, csd1, packet_mod, 65);
                        FusionCodec.Encoder.WriteDchData(false, csd2, packet_mod, 65);
                    }
                }
                else
                {
                    logger.Error($"Unable to decode header (DCH-2), packet : {Convert.ToHexString(packet)}");
                }
            }
            else
            {
                logger.Error($"Unable to decode header (DCH-1), packet : {Convert.ToHexString(packet)}");
            }
        } 
        else if (fi == FrameInformation.CC) 
        {   // communication channel
            if (fn == 1 && dt == DataType.VD_MODE_2 && _cfg.DgIdMgmt.Prefix > 0)
            {
                packet_mod = new byte[155];
                packet.CopyTo(packet_mod, 0);
                string src = $"{client.DgId}/{tx.SourceClean}".PadRight(10);
                FusionCodec.Encoder.WriteVDMode2Data(Encoding.ASCII.GetBytes(src), packet_mod, 65);
            }
            if (dt == DataType.VD_MODE_2 && tx.GwId != 0)
            {
                byte[]? data = FusionCodec.Decoder.DecodeVD2(packet, 65);
                if (data != null)
                {
                    string dch = DecodeDch(data).Trim();
                    switch (fn)
                    {
                        case 4:
                            if (dch.Length > 0 && dch != tx.Rem12)
                            {
                                tx.Rem12 = dch;
                                logger.Debug($"<{tx.StreamId:D7}> Additional information Rem1+2: {dch}");
                                // JSON omitted
                            }
                            break;
                        case 5:
                            if (dch.Length > 0 && dch != tx.Rem34)
                            {
                                tx.Rem34 = dch;
                                logger.Debug($"<{tx.StreamId:D7}> Additional information Rem3+4: {dch}");
                                // JSON omitted
                            }
                            break;
                        case 6:
                        case 7:
                            // GPS gym goes here ...
                            //tx.RadioCode = radioCode;
                            //tx.CoordSend = coordSend;
                            break;
                    }
                }
            }
        }

        int id_corr = client.GwId;
        string gw_corr = client.Callsign;
        bool txOk = true;

        if (tx.GwId == 0)
        {   // LAST CHANCE - new YsfdStream (reused/old streams have a GwId > 0)
            if (dt == DataType.DATA_FR && (ft is 1 or 2))
            {
                txOk = false;
            }
            else if (_accessControl.IsGatewayBlocked(gw_corr) || _accessControl.IsGatewayLocked(gw_corr))
            {
                txOk = false;
            }
            else if (_accessControl.IsIpBlocked(endPoint) || _accessControl.IsIpLocked(endPoint))
            {
                txOk = false;
            }
            else if (cm == CallMode.RadioMode
                && _dstCallsign.Length == 10
                && _dstCallsign != "**********"
                && _accessControl.IsSerialBlocked(_dstCallsign[5..10]))
            {
                txOk = false;
            }
            else 
            {
                string cs = Encoding.ASCII.GetString(packet, 14, 10).Trim(); // TODO: take this from tx object?
                txOk = _accessControl.CanTransmit(cs, _cfg.ClientMgmt.CheckRE);
            }
        }

        if (txOk) 
        {
            // DG-ID gym
            if (fi == FrameInformation.HC && _cfg.DgIdMgmt.DgIdList.Contains(sq)) 
            {
                if (auxDgid == 0 && client.DgId != sq && client.THold == 0)
                {
                    logger.Debug($"Changing DG-ID for {client.Callsign} from {client.DgId} to {sq}");
                    client.DgId = sq;
                    tx.IsLocal = true;
                    tx.StreamType = 1;
                    _gwDgIdMap[client.Callsign] = sq; // remember DG-ID
                }
                else if (fi == FrameInformation.HC && sq != 0)
                {
                    logger.Error($"Invalid DG-ID: {sq}"); // TODO: provide more information
                }
            }

            if (fi == FrameInformation.HC && tx.GwId == 0 && id_corr != 0 && sq != 127 && (!dgIdBusy || client.DgId == _cfg.DgIdMgmt.DgIdLocal))
            {   // new Ysfd stream (no reuse)
                tx.GwId = id_corr;
                tx.Timeout = 0f;
                tx.Gateway = Encoding.ASCII.GetString(packet, 4, 10).Trim(); // TODO: already done in FromPacket()
                tx.Source = Encoding.ASCII.GetString(packet, 24, 10).Trim(); // TODO: already done in FromPacket() 
                tx.SourceClean = CleanCallsign(tx.Source);  // TODO: already done in FromPacket()
                tx.Destination = Encoding.ASCII.GetString(packet, 34, 10).Trim();  // TODO: already done in FromPacket()
                tx.StreamId = random.Next(0, 10_000_000);
                tx.StartTime = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000f);
                tx.Latitude = 999.0;
                tx.Longitude = 999.0;
                if (client.DgId == _cfg.DgIdMgmt.DgIdLocal)
                {   // local
                    tx.DgId = -tx.GwId;
                    tx.IsLocal = true;
                }
                else
                {
                    tx.DgId = client.DgId;
                }
                _activeStreams.Add(tx);
                //string dst = (_dstCallsign.Length == 0 || _dstCallsign == "**********") ? tx.Destination : _dstCallsign;

                // TODO: check wild PTT
            }
        }
        else
        {
            if (!_rxLock.Contains(id_corr))
                _rxLock.Add(id_corr);
            
            _rxLockTout[id_corr] = 0f;
        }

        if (id_corr == tx.GwId && id_corr != 0)
            tx.Timeout = 0f;

        if (!tx.IsLocal)
        {
            foreach (var c in _clientMap.Values)
            {
                if (c.EndPoint != endPoint 
                    && id_corr != 0 && id_corr == tx.GwId 
                    && !_rxLock.Contains(id_corr) 
                    && !c.IsLocked 
                    && c.DgId == client.DgId)
                {
                    if (c.THold < 0)
                        SendDatagram(packet, packet.Length, c.EndPoint);
                    else
                        if (packet_mod != null)
                            SendDatagram(packet_mod, packet_mod.Length, c.EndPoint);
                }
            }
        }

        if (fi == FrameInformation.TC && tx.GwId != 0 && tx.GwId == id_corr)
        {
            _activeStreams.Remove(tx);
        }

    }

    internal void HandleYsfi(IPEndPoint endPoint, byte[] packet)
    {
        logger.Debug($"");
    }

    internal void HandleYsfo(IPEndPoint endPoint, byte[] packet)
    {
        logger.Debug($"");
        // TODO: extract DG-IDs and process
    }

    /// <summary>
    /// Handles an incoming YSFP (YSF Reflector Front-end) packet from a gateway client.This method performs the following steps:
    /// <list type="bullet">
    ///   <item><description>Extracts the gateway callsign from the packet.</description></item>
    ///   <item><description>Checks if the client is already registered. If so, updates its last seen timestamp.</description></item>
    ///   <item><description>If the client is new, creates a <see cref="Client"/> instance and assigns a gateway ID.</description></item>
    ///   <item><description>Applies blocked or locked status based on callsign or endpoint.</description></item>
    ///   <item><description>Resolves the client's DG-ID using a cascade:
    ///     <list type="bullet">
    ///       <item><description>Static DG-ID from callsign suffix or auxDgid placeholder.</description></item>
    ///       <item><description>Home DG-ID entry, if it exists (fixed or timed).</description></item>
    ///       <item><description>Database lookup for the callsign (if DG-ID is still 0 and THold is 0).</description></item>
    ///       <item><description>Fallback to configured default DG-ID if none found.</description></item>
    ///     </list>
    ///   </description></item>
    ///   <item><description>Sanity check ensures that the final DG-ID is in the configured DG-ID whitelist; otherwise, assigns the default DG-ID.</description></item>
    ///   <item><description>Sends the YSFP registration/acknowledgment back to the client.</description></item>
    /// </list>
    /// </summary>
    /// <param name="endPoint">The <see cref="IPEndPoint"/> representing the client's network address and port.</param>
    /// <param name="packet">The raw byte array of the incoming YSFP packet.</param>
    /// <remarks>
    /// - The method ensures that any assigned DG-ID is valid and whitelisted.
    /// - The auxDgid placeholder allows for future special-socket DG-ID assignments.
    /// - Clients without a DG-ID or HomeDgidEntry will receive the default DG-ID, unless 0 is whitelisted.
    /// - This method mirrors the DG-ID assignment and client handling logic of the original Python YSFReflector.
    /// </remarks>
    internal void HandleYsfp(IPEndPoint endPoint, byte[] packet)
    {
        string callsign = Encoding.ASCII.GetString(packet, 4, 10).Trim(); // GW callsign
        logger.Debug($"endPoint={endPoint}, callsign={callsign}");

        int auxDgid = 0; // usually assigned to socket, currently a placeholder/reminder for future extension

        if (_clientMap.TryGetValue(endPoint, out Client? c))
        {
            // TODO: check if callsign is still the same?
            c.LastSeen = DateTime.UtcNow;
        }
        else
        {
            // New client
            Client newClient = new(endPoint, callsign) { GwId = _nextGwId++ };
            _clientMap[endPoint] = newClient;
            logger.Debug($"New client: {newClient}");

            // Restrictions
            newClient.IsListenOnly = _accessControl.IsGatewayBlocked(callsign) || _accessControl.IsIpBlocked(endPoint);
            if (_accessControl.IsGatewayLocked(callsign) || _accessControl.IsIpLocked(endPoint))
            {
                newClient.IsListenOnly = true;
                newClient.IsLocked = true;
            }

            // DG-ID decision cascade
            newClient.DgId = auxDgid > 0 ? auxDgid : DgIdFromCallsign(callsign).Item2;

            if (newClient.DgId > 0 && _cfg.DgIdMgmt.DgIdList.Contains(newClient.DgId))
            {
                newClient.THold = THoldStatus.Static;
                logger.Debug($"Static DG-ID ({newClient.DgId}) via port or gw name");
            }
            else
            {
                HomeDgidEntry? hde = _homeDgIds.GetHomeDgidEntry(callsign);
                if (hde != null)
                {
                    newClient.DgId = hde.Dgid;
                    if (hde.Time < 0)
                    {
                        newClient.THold = THoldStatus.FixedHome;
                        logger.Debug($"Static DG-ID ({newClient.DgId}) via back to home");
                    }
                    else
                    {
                        // Back to home active
                        newClient.DgIdHome = hde.Dgid;
                        newClient.DgIdTimeMinutes = hde.Time;
                    }
                }
                if (newClient.DgId == 0 && newClient.THold == 0 )
                {
                    // try to read DG-ID from DB with callsign
                    // - if (callsign not in DB) newClient.Dgid = _cfg.DgidMgmt.DgidDefault;
                }
            }

            // Final sanity check (all DG-IDs assigned must be a member of the configured DG-ID list, otherwise the default DG-ID is used!)
            if (!_cfg.DgIdMgmt.DgIdList.Contains(newClient.DgId))
            {
                logger.Debug($"Invalid DG-ID ({newClient.DgId}) changed to default ({_cfg.DgIdMgmt.DgIdDefault})");
                newClient.DgId = _cfg.DgIdMgmt.DgIdDefault;
                newClient.THold = THoldStatus.Dynamic;
            }
            SendYsfp(endPoint);
        }
    }

    internal void HandleYsfs(IPEndPoint endPoint)
    {
        logger.Debug("");
        SendYsfs(endPoint);
    }

    /// <summary>
    /// The method performs a logoff (unlink) request of a client identified by its endpoint. 
    /// </summary>
    /// <param name="endPoint">The endpoint of the client to unlink.</param>
    internal void HandleYsfu(IPEndPoint endPoint)
    {
        logger.Debug($"endPoint={endPoint}");
        if (_clientMap.TryRemove(endPoint, out Client? c))
        {
            logger.Debug($"Unlinked client: {c}");
        }
        else
        {
            logger.Debug($"No client with this endPoint={endPoint} registered.");
        }
    }

    internal void HandleYsfv(IPEndPoint endPoint, byte[] packet)
    {
        logger.Debug($"");
    }

    private void ProcessRcvdPackets()
    {
        while (_isRunning)
        {
            if (_rcvdPackets.TryTake(out (IPEndPoint, byte[]) item, -1))
            {
                IPEndPoint endPoint = item.Item1;
                byte[] packet = item.Item2;
                //if (_accessControl.IsIpBlocked(endPoint))
                //{
                //    // All blocked 'end points' are rejected here
                //    logger.Warn($"Client is blocked by endPoint = {endPoint}");
                //    continue;
                //}
        
                string signature = Encoding.ASCII.GetString(packet, 0, 4);
                switch (signature)
                {
                    case "YSFD" when packet.Length == 155:
                        HandleYsfd(endPoint, packet);
                        break;
                    case "YSFI":  // TODO: len check
                        HandleYsfi(endPoint, packet);
                        break;
                    case "YSFO":  // TODO: len check
                        HandleYsfo(endPoint, packet);
                        break;
                    case "YSFP" when packet.Length == 14:
                        HandleYsfp(endPoint, packet);
                        break;
                    case "YSFS":
                        HandleYsfs(endPoint);
                        break;
                    case "YSFU":
                        HandleYsfu(endPoint);
                        break;
                    case "YSFV":
                        HandleYsfv(endPoint, packet);
                        break;
                    default:
                        logger.Warn($"Unexpected packet, signature {signature}, packet = {Convert.ToHexString(packet)}, len = {packet.Length} - packet ignored");
                        break;
                }
            }
            else
            {   
                logger.Error("Unable to read from packet Queue");
            }
        }
        logger.Error("Process packets thread exiting.");
    }

    private void ReadUdpPackets()
    {
        logger.Debug("UDP receive thread started");
        try
        {
            while (_isRunning && _udpClient != null)
            {
                IPEndPoint remoteEp = new(IPAddress.Any, 0);
                byte[]? packet = _udpClient.Receive(ref remoteEp); // This line blocks until a packet arrives
                _rcvdPackets.Add((remoteEp, packet));
                //logger.Trace($"Received packet {Convert.ToHexString(packet)} from {remoteEp}");
            }
        }
        catch (SocketException ex)
        {
            if (_isRunning)
                logger.Error(ex, "Socket receive error");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error");
        }

        logger.Debug("UDP receive thread exiting");
    }

    /// <summary>
    /// Send datagram to the specified remote endpoint
    /// </summary>
    internal void SendDatagram(byte[] datagram, int len, IPEndPoint ep)
    {
        _udpClient?.Send(datagram, len, ep);
    }

    internal void SendYsfp(IPEndPoint ep)
    {
        logger.Debug("");
        byte[] msg = Encoding.ASCII.GetBytes("YSFPREFLECTOR ");
        SendDatagram(msg, msg.Length, ep);
    }

    internal void SendYsfs(IPEndPoint ep)
    {
        logger.Debug("");
        StringBuilder sb = new();
        sb.Append("YSFS").Append(_cfg.Info.ReflectorId).Append(_cfg.Info.Description.PadRight(30)).Append(_clientMap.Count.ToString("D3"));
        byte[] msg = Encoding.ASCII.GetBytes(sb.ToString());
        SendDatagram(msg, msg.Length, ep);
    }

    /// <summary>
    /// Start running this reflector instance.
    /// </summary>
    public void Start()
    {
        logger.Debug($"{_isRunning}");

        if (_isRunning)
            return;

        string json = File.ReadAllText(GW_DGID_MAP_FILE);
        var map = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
        if (map != null) 
            _gwDgIdMap = map;

        // Create the UDP socket used by this YSF client
        logger.Debug($"Reflector: {_cfg.Network.IpAddr}:{_cfg.Network.Port}");
        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(_cfg.Network.IpAddr), _cfg.Network.Port))
        {
            Client = { ReceiveTimeout = 0 } // no timeout
        };

        logger.Debug($"Local socket bound to {_cfg.Network.IpAddr}:{_cfg.Network.Port}");

        _isRunning = true;

        _udpReaderThread = new Thread(ReadUdpPackets) { IsBackground = true, Name = "UDP-Reader" };
        _udpReaderThread.Start();

        _procRcvdPacketThread = new Thread(ProcessRcvdPackets) { IsBackground = true, Name = "UDP-Proc" };
        _procRcvdPacketThread.Start();

        _checkClientInactivityTimer.Start();

        logger.Info("Reflector started.");
        _isRunning = true;
    }

    /// <summary>
    /// Stop running this reflector instance.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
            return;

        logger.Debug("Stopping reflector...");

        _isRunning = false;
        _udpClient?.Close();
        _udpClient = null;

        _udpReaderThread?.Join(500);
        _udpReaderThread = null;

        _rcvdPackets.CompleteAdding();

        _checkClientInactivityTimer.Stop();

        string json = JsonSerializer.Serialize(_gwDgIdMap, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GW_DGID_MAP_FILE, json);

        logger.Info("Reflector stopped.");
    }


    readonly object _streamLock = new();
    readonly object _rxLockSync = new();

    private void StreamInactvitiyTimerCallback()
    {
        foreach (var stream in _activeStreams.ToList()) // safe snapshot
        {
            if (stream.Timeout < 5)
                stream.Timeout += 0.1f;

            if (stream.Timeout > 2f && stream.GwId != 0)
            {
                //Gps.Reset();
                _activeStreams.Remove(stream);
                logger.Debug($"<{stream.StreamId:D7}> Network watchdog expired at {stream.Latitude:F6}, {stream.Longitude:F6}");
            }

            // TX timeout condition (TX too long -> temp. block callsign)
            if ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - stream.StartTime) > _cfg.Protections.Timeout && stream.GwId != 0)
            {
                if (!_accessControl.IsCallsignTempBlocked(stream.Source)) // TODO: ! OK?
                {
                    int time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + _cfg.Protections.Reactivate;
                    _accessControl.AddTempBlockedCallsign(stream.Source, BlockReason.TxTimeout, time);
                    logger.Debug($"callsign {stream.Source} blocked for {_cfg.Protections.Timeout} due to TX timeout.");
                    _activeStreams.Remove(stream);
                }
            }
        }

        // Process RX locks
        var removeList = new List<int>();
        lock (_rxLockSync)
        {
            foreach (var kvp in _rxLockTout.ToList())
            {
                _rxLockTout[kvp.Key] = kvp.Value + 0.1f;
                if (_rxLockTout[kvp.Key] > 1.5)
                {
                    removeList.Add(kvp.Key);
                    _rxLock.Remove(kvp.Key);
                }
            }

            removeList.ForEach(k => _rxLockTout.Remove(k));
        }

    }
}