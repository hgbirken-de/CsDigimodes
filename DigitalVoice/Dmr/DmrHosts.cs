
using NLog;

namespace DigitalVoice.Dmr;

/// <summary>
/// Utility class to load and query DMR host records (e.g., BrandMeister or FreeDMR). The records 
/// are stored in a static dictionary keyed by the designator (e.g. "BM_2001_Europe_HAMNET").
/// </summary>
public static class DmrHosts
{
    const string fileName = "DMR_Hosts.txt";

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static DmrHosts()
    {
        LoadDataFromEmbeddedResource();
    }

    /// <summary>
    /// Internal storage for all parsed hosts.
    /// Key: Designator string (e.g. "BM_2001_Europe_HAMNET").
    /// Value: Tuple containing (dmrId, host, port, password).
    /// </summary>
    public readonly static Dictionary<string, (int dmrid, string Host, int Port, string password)> _hosts = [];

    /// <summary>
    /// Returns host information for a given key (designator).
    /// If the key does not exist, returns a tuple of default values.
    /// </summary>
    /// <param name="designator">Designator string (e.g. "BM_2001_Europe_HAMNET").</param>
    /// <returns>Tuple (dmrId, host, port, password).</returns>
    public static (int Dmrid, string Host, int Port, string Password) GetHostInfo(string designator)
    {
        if (_hosts != null && _hosts.TryGetValue(designator, out var info))
        {
            return info;
        }
        else 
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to retrieve host information for the given YSF designator.
    /// </summary>
    /// <param name="designator">The reflector designator (e.g. "20201") used as the lookup key.</param>
    /// <param name="info">A tuple with information about the reflector, if reflector was found.</param>
    /// <returns><c>true</c> if the reflector was found in the dictionary; otherwise <c>false</c>.</returns>
    public static bool TryGetHostInfo(string designator, out (int Dmrid, string Host, int Port, string Password) info)
    {
        if (_hosts.TryGetValue(designator, out info))
        {
            return true;
        }

        info = default;
        return false;
    }

    /// <summary>
    /// Loads the DMR hosts file and parses its entries into the internal dictionary.
    /// Expected file format (whitespace separated):
    /// BM_2001_Europe_HAMNET    2001    44.148.230.201    passw0rd    62031
    ///
    /// Columns:
    /// [0] Designator (must start with "BM_" or "FreeDMR_")
    /// [1] DMR ID
    /// [2] Host (IP or hostname)
    /// [3] Password
    /// [4] Port
    /// </summary>
    /// <param name="filePath">Path to the hosts file.</param>
    /// <returns>Dictionary of parsed hosts.</returns>
    public static Dictionary<string, (int Dmrid, string Host, int Port, string Password)> LoadData(string filePath)
    {
        logger.Debug($"file_path = {filePath}");

        _hosts.Clear();
        foreach (var line in File.ReadLines(filePath))
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                continue;

            var parts = trimmedLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 5)
            {
                var designator = parts[0].Trim();

                if (!designator.StartsWith("BM_") && !designator.StartsWith("FreeDMR_"))
                    continue;

                var host = parts[2].Trim();
                var pwd = parts[3].Trim();

                if (!int.TryParse(parts[1].Trim(), out var dmrId))
                {
                    logger.Warn($"Invalid DMR Id in line: {line}");
                    continue;
                }
                if (!int.TryParse(parts[4].Trim(), out var port))
                {
                    logger.Warn($"Invalid port value in line: {line}");
                    continue;
                }
                _hosts[designator] = (dmrId, host, port, pwd);
            }
        }

        return _hosts;
    }

    /// <summary>
    /// Loads DMR host data from an embedded resource file and populates the internal host dictionary.
    /// </summary>
    /// <remarks>
    public static void LoadDataFromEmbeddedResource()
    {
        logger.Debug($"");

        // Get the assembly that actually contains the resource
        var assembly = typeof(DmrHosts).Assembly;

        string? ns = typeof(DmrHosts).Namespace;

        if (string.IsNullOrEmpty(ns))
            throw new InvalidOperationException("Namespace of DmrHosts is null or empty.");

        string resourceName = $"{ns}.{fileName}";

        // Check available resources
        var names = assembly.GetManifestResourceNames();

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Resource '{resourceName}' not found. Available: {string.Join(", ", names)}");
        using var reader = new StreamReader(stream);

        _hosts.Clear();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                continue;

            var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 5)
            {
                var designator = parts[0].Trim();

                if (!designator.StartsWith("BM_") && !designator.StartsWith("FreeDMR_"))
                    continue;

                var host = parts[2].Trim();
                var pwd = parts[3].Trim();

                if (!int.TryParse(parts[1].Trim(), out var dmrId))
                {
                    logger.Warn($"Invalid port value in line: {line}");
                    continue;
                }
                if (!int.TryParse(parts[4].Trim(), out var port))
                {
                    logger.Warn($"Invalid port value in line: {line}");
                    continue;
                }
                _hosts[designator] = (dmrId, host, port, pwd);
            }
        }
    }

    /// <summary>
    /// Returns all known DMR host mappings.
    /// </summary>
    public static IReadOnlyDictionary<string, (int Dmrid, string Host, int Port, string Password)> All => _hosts;
}