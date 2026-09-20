using NLog;
using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;

namespace YsfReflector;

public class AccessControl
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly string _filePath;

    private readonly object _lock = new();

    private readonly HashSet<string> _allowedCallsigns = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _blockedCallsigns = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _blockedGateways = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _lockedGateways = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _blockedIps = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _lockedIps = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _blockedSuffixes = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _blockedSerials = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, (BlockReason, int)> _tempBlockedCallsigns = [];

    private readonly ConcurrentDictionary<IPEndPoint, Client> _clientMap;

    public AccessControl(string filePath, ConcurrentDictionary<IPEndPoint, Client> clientMap)
    {
        _filePath = filePath;
        _clientMap = clientMap;
        Load();
        WatchForChanges();
    }

    internal void AddTempBlockedCallsign(string callsign, BlockReason reason, int time)
    {
        _tempBlockedCallsigns[callsign] = (reason, time);
    }

    internal void AdjustTempBlockedCallsigns()
    {
        int time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var toDelete = _tempBlockedCallsigns.Where(kvp => time >= kvp.Value.Item2).Select(kvp => kvp.Key).ToList();
        toDelete.ForEach(k => _tempBlockedCallsigns.Remove(k));
    } 

    public bool IsCallsignTempBlocked(string cs) => _tempBlockedCallsigns.ContainsKey(cs);
    public bool IsCallsignAllowed(string cs) => _allowedCallsigns.Contains(cs);
    public bool IsCallsignBlocked(string cs) => _blockedCallsigns.Contains(cs);
    public bool IsGatewayBlocked(string gw) => _blockedGateways.Contains(gw);
    public bool IsGatewayLocked(string gw) => _lockedGateways.Contains(gw);
    public bool IsSuffixBlocked(string suf) => _blockedSuffixes.Contains(suf);
    public bool IsSerialBlocked(string ser) => _blockedSerials.Contains(ser);

    public bool IsIpBlocked(IPEndPoint ep)
    {
        bool result = _blockedIps.Contains(ep.Address.ToString());
        if (result) return true;

        return _blockedIps.Contains($"{ep.Address}:{ep.Port}");
    }

    public bool IsIpLocked(IPEndPoint ep)
    {
        bool result = _lockedIps.Contains(ep.Address.ToString());
        if (result) return true;

        return _lockedIps.Contains($"{ep.Address}:{ep.Port}");
    }

    /// <summary>
    /// Determines whether a given callsign is allowed to transmit based on
    /// whitelist, blacklist, suffix, and optional regex validation rules.
    /// </summary>
    /// <param name="callsign">The callsign to validate. May include suffixes (e.g. "DL1ABC/R").</param>
    /// <param name="regexMode">Regular Expression Enforcement Mode:
    /// <list type="bullet">
    /// <item><description><b>1</b> – Strict mode: whitelist and valid amateur callsigns only.</description></item>
    /// <item><description><b>0</b> – Relaxed mode: whitelisted or not blacklisted.</description></item>
    /// <item><description><b>-1</b> – Default deny: only whitelisted callsigns pass.</description></item>
    /// </list>
    /// </param>
    /// <returns><c>true</c> if transmission is allowed; otherwise <c>false</c>.</returns>
    public bool CanTransmit(string callsign, int reEn = 0)
    {
        if (string.IsNullOrWhiteSpace(callsign))
            return false;

        // Split callsign and suffix (like "DL1ABC/N")
        var parts = Regex.Split(callsign.ToUpperInvariant().Trim(), @"[-/'\s]+");
        var baseCall = parts[0];
        string? suffix = parts.Length > 1 ? parts[1] : null;

        if (_tempBlockedCallsigns.ContainsKey(baseCall)) 
            return false;

        // 1️ - Block repeater/node/gateway suffixes
        if (!string.IsNullOrEmpty(suffix) && IsSuffixBlocked(suffix))
            return false;

        // 2️ - Block explicitly blacklisted callsigns
        if (IsCallsignBlocked(baseCall))
            return false;

        // 3️ - Check regex / whitelist / blacklist logic
        switch (reEn)
        {
            case 1: // strict validation mode
                if (IsCallsignAllowed(baseCall))
                    return true;
                if (IsCallsignBlocked(baseCall))
                    return false;
                // Regex: valid amateur callsign pattern, up to 8 chars
                if (Regex.IsMatch(baseCall, @"^\d?[A-Z]{1,2}\d{1,4}[A-Z]{1,3}$", RegexOptions.IgnoreCase) && baseCall.Length <= 8)
                    return true;
                return false;

            case 0: // whitelist + blacklist mode
                if (IsCallsignAllowed(baseCall))
                    return true;
                if (IsCallsignBlocked(baseCall))
                    return false;
                return true; // default allow

            case -1: // default deny mode
                if (IsCallsignBlocked(baseCall))
                    return false;
                if (IsCallsignAllowed(baseCall))
                    return true;
                return false; // default deny

            default:
                return false;
        }
    }


    public void Load()
    {
        lock (_lock)
        {
            _allowedCallsigns.Clear();
            _blockedCallsigns.Clear();
            _blockedGateways.Clear();
            _lockedGateways.Clear();
            _blockedIps.Clear();
            _lockedIps.Clear();
            _blockedSuffixes.Clear();
            _blockedSerials.Clear();

            if (!File.Exists(_filePath))
            {
                logger.Warn($"File not found: {_filePath}");
                return;
            }

            logger.Info($"loading file {_filePath}");

            var lines = File.ReadAllLines(_filePath).Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith('#'));

            foreach (var line in lines)
            {
                string[] token = line.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (token.Length < 2)
                {
                    logger.Error($"Unable to parse: line = {line}");
                    continue; // invalid record
                }

                string k = token[0].Trim().ToUpper();
                string v = token[1].Trim();
                switch (k)
                {
                    case "AL": // allowed (white list)
                        _allowedCallsigns.Add(v);
                        break;
                    case "CS": // CS blocked
                        _blockedCallsigns.Add(v);
                        break;
                    case "GWB": // GW blocked
                        _blockedGateways.Add(v);
                        break;
                    case "GWL": // GW locked
                        _lockedGateways.Add(v);
                        break;
                    case "IPB": // IP blocked
                    case "IPL": // IP locked
                        try
                        {
                            string ipString = token.Length == 3 ? $"{token[1]}:{token[2]}" : $"{token[1]}";
                            if ("IPB".Equals(k))
                                _blockedIps.Add(ipString);
                            else
                                _lockedIps.Add(ipString);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"Unable to parse: line = {line}");
                        }
                        break;
                    case "SUF": // suffix blocked
                        _blockedSuffixes.Add(v);
                        break;
                    case "SNB": // serial number blocked
                        _blockedSerials.Add(v);
                        break;
                    default:
                        logger.Error($"Unable to parse: line = {line}");
                        break;
                }
            }

            logger.Info($"Access lists loaded: AL={_allowedCallsigns.Count}, BL={_blockedCallsigns.Count}, GW={_blockedGateways.Count}, IP={_blockedIps.Count}");
        }
    }

    private void Reload()
    {
        const int n = 5;
        for (int i = 0; i < n; i++)
        {
            try
            {
                Load();
                return;
            }
            catch (IOException)
            {
                logger.Warn("Access control file locked, retrying...");
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error while reloading access control file.");
                return;
            }
        }
        logger.Error($"Failed to reload access control file after {n} attempts.");
    }

    private void WatchForChanges()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (dir is null) return;

        var watcher = new FileSystemWatcher(dir)
        {
            Filter = Path.GetFileName(_filePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        watcher.Changed += (s, e) =>
        {
            logger.Info("Access control file changed, reloading...");
            Thread.Sleep(200); // wait a bit for the editor to finish writing
            Reload();
            UpdateClients(); // re-evaluate all clients
        };
        watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Updates all connected client's access control status based on the current block and lock lists.
    /// </summary>
    /// <remarks>
    /// This method iterates over all registered clients and updates their <see cref="IsListenOnly"/> and 
    /// <see cref="IsLocked"/> flags according to the configured access control rules:
    /// <list type="bullet">
    /// <item><description><b>IsListenOnly</b> is set to <c>true</c> if the client is either blocked or locked (by gateway or IP).</description></item>
    /// <item><description><b>IsLocked</b> is set to <c>true</c> only if the client is locked (by gateway or IP).</description></item>
    /// </list>
    /// Clients marked as "listen-only" can receive traffic but are not allowed to transmit.
    /// Locked clients are additionally restricted from participating in any active stream.
    /// </remarks>
    public void UpdateClients()
    {
        logger.Debug("");
        foreach (var c in _clientMap.Values)
        {
            c.IsListenOnly = IsGatewayBlocked(c.Callsign) || IsIpBlocked(c.EndPoint) || IsGatewayLocked(c.Callsign) || IsIpLocked(c.EndPoint);
            c.IsLocked = IsGatewayLocked(c.Callsign) || IsIpLocked(c.EndPoint);
        }
    }
}