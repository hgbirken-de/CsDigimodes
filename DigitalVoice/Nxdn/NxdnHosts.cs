
using NLog;

namespace DigitalVoice.Nxdn;

/// <summary>
/// Utility class to load and query NXDN host records. The records are stored 
/// in a static dictionary keyed by the designator.
/// </summary>
public static class NxdnHosts
{
    const string fileName = "NXDNHosts.txt";

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static NxdnHosts()
    {
        LoadDataFromEmbeddedResource();
    }

    /// <summary>
    /// Internal storage for all parsed hosts.
    /// Key: Id.
    /// Value: Tuple containing (host, port).
    /// </summary>
    public readonly static Dictionary<int, (string Host, int Port)> _hosts = [];

    /// <summary>
    /// Returns host information for a given Id.
    /// If the key does not exist, returns a tuple of default values.
    /// </summary>
    /// <param name="id">.</param>
    /// <returns>Tuple (host, port).</returns>
    public static (string Host, int Port) GetHostInfo(int id)
    {
        if (_hosts.TryGetValue(id, out var info))
        {
            return info;
        }
        else 
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to retrieve host information given the NXDN reflector Id.
    /// </summary>
    /// <param name="id">The reflector designator (e.g. "20201") used as the lookup key.</param>
    /// <param name="info">A tuple with host address and port, or a default tuple if no found.</param>
    /// <returns><c>true</c> if the host was found in the dictionary; otherwise <c>false</c>.</returns>
    public static bool TryGetHostInfo(int id, out (string Host, int Port) info)
    {
        if (_hosts.TryGetValue(id, out info))
        {
            return true;
        }

        info = default;
        return false;
    }

    /// <summary>
    /// Loads a NXDN hosts file and parses its entries into the internal dictionary.
    /// Expected file format (whitespace separated):
    /// ID   44.148.230.201  62031
    ///
    /// Columns:
    /// [0] Id 
    /// [1] Host (IP or hostname)
    /// [2] Port
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

            var parts = trimmedLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                if (!int.TryParse(parts[0].Trim(), out var id))
                {
                    logger.Warn($"Invalid Id in line: {line}");
                    continue;
                }

                var host = parts[1].Trim();

                if (!int.TryParse(parts[2].Trim(), out var port))
                {
                    logger.Warn($"Invalid port value in line: {line}");
                    continue;
                }
                _hosts[id] = (host, port);
            }
        }
    }

    /// <summary>
    /// Loads NXDN host data from an embedded resource file and populates the internal host dictionary.
    /// </summary>
    /// <remarks>
    public static void LoadDataFromEmbeddedResource()
    {
        logger.Debug($"");

        // Get the assembly that actually contains the resource
        var assembly = typeof(NxdnHosts).Assembly;

        string? ns = typeof(NxdnHosts).Namespace;

        if (string.IsNullOrEmpty(ns))
            throw new InvalidOperationException("Namespace of NxdnHosts is null or empty.");

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
            if (parts.Length >= 3)
            {
                if (!int.TryParse(parts[0].Trim(), out var id))
                {
                    logger.Warn($"Invalid Id value in line: {line}");
                    continue;
                }

                var host = parts[1].Trim();

                if (!int.TryParse(parts[2].Trim(), out var port))
                {
                    logger.Warn($"Invalid port value in line: {line}");
                    continue;
                }
                _hosts[id] = (host, port);
            }
        }
    }

    /// <summary>
    /// Returns all known NXDN host mappings.
    /// </summary>
    public static IReadOnlyDictionary<int, (string Host, int Port)> All => _hosts;
}