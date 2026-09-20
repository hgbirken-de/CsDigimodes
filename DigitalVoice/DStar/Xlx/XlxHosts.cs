using NLog;

namespace DigitalVoice.DStar.Xlx;

/// <summary>
/// Utility class to load and query XLX host records. The records are stored 
/// in a static dictionary keyed by the designator (e.g. "XLX001").
/// </summary>
public static class XlxHosts
{
    const string fileName = "XLXhosts.csv";

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static XlxHosts()
    {
        LoadDataFromEmbeddedResource();
    }

    /// <summary>
    /// Internal storage for all parsed hosts.
    /// Key: Designator string (e.g. "XLX001").
    /// Value: Tuple containing (IP Addr, Dashboard URL, Country, Comment).
    /// </summary>
    public readonly static Dictionary<string, (string IpAddr, string DashboardUrl, string Country, string Comment)> _hosts = [];

    /// <summary>
    /// Returns host information for a given key (designator).
    /// If the key does not exist, returns a tuple of default values.
    /// </summary>
    /// <param name="designator">Designator string (e.g. "BM_2001_Europe_HAMNET").</param>
    /// <returns>Tuple (IpAddr, Dashboard URL, Country, Comment).</returns>
    public static (string IpAddr, string DashboardUrl, string Country, string Comment) GetHostInfo(string designator)
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
    public static bool TryGetHostInfo(string designator, out (string IpAddr, string DashboardUrl, string Country, string Comment) info)
    {
        if (_hosts.TryGetValue(designator, out info))
        {
            return true;
        }

        info = default;
        return false;
    }

    /// <summary>
    /// Loads the XLX hosts file and parses its entries into the internal dictionary.
    /// Columns:
    /// [0] ReflectorID
    /// [1] IP Address
    /// [2] Dashboard URL
    /// [3] Country
    /// [4] Comment
    /// </summary>
    /// <param name="filePath">Path to the hosts file.</param>
    public static void LoadData(string filePath)
    {
        logger.Debug($"file_path = {filePath}");

        _hosts.Clear();
        foreach (var line in File.ReadLines(filePath))
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                continue;

            var parts = trimmedLine.Split(',');
            if (parts.Length >= 5)
            {
                var id = parts[0].Trim();
                var ipAddr = parts[1].Trim();
                var dashboardUrl = parts[2].Trim();
                var country = parts[3].Trim();
                var comment = parts[4].Trim();
                _hosts[id] = (ipAddr, dashboardUrl, country, comment);
            }
        }
    }

    /// <summary>
    /// Loads XLX host data from an embedded resource file and populates the internal host dictionary.
    /// </summary>
    /// <remarks>
    /// <exception cref="InvalidOperationException">Thrown if the namespace of <see cref="XlxHosts"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the embedded resource cannot be found in the assembly.</exception>
    public static void LoadDataFromEmbeddedResource()
    {
        logger.Debug($"");

        // Get the assembly that actually contains the resource
        var assembly = typeof(XlxHosts).Assembly;

        string? ns = typeof(XlxHosts).Namespace;

        if (string.IsNullOrEmpty(ns))
            throw new InvalidOperationException("Namespace of XlxHosts is null or empty.");

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

            var parts = line.Split(',');
            if (parts.Length >= 5)
            {
                var id = parts[0].Trim();
                var ipAddr = parts[1].Trim();
                var dashboardUrl = parts[2].Trim();
                var country = parts[3].Trim();
                var comment = parts[4].Trim();
                _hosts[id] = (ipAddr, dashboardUrl, country, comment);
            }
        }
    }

    /// <summary>
    /// Returns all known XLX host mappings.
    /// </summary>
    public static IReadOnlyDictionary<string, (string IpAddr, string DashboardUrl, string Country, string Comment)> All => _hosts;
}