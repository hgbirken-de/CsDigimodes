using NLog;

namespace DigitalVoice.DStar.Xrf;

/// <summary>
/// Provides access to XRF reflector host addresses.
/// </summary>
public static class XrfHosts
{
    const string fileName = "DExtra_Hosts.txt";

    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // Dictionary to store reflector name → IP mapping
    private static readonly Dictionary<string, string> _hosts = new(StringComparer.OrdinalIgnoreCase);

    static XrfHosts()
    {
        LoadDataFromEmbeddedResource();
    }

    /// <summary>
    /// Loads reflector host data from a file. Each non-comment line must be in the format: DESIGNATOR  SPACE  IP_ADDRESS
    /// </summary>
    /// <param name="filePath">The path to the DExtra_Hosts.txt file.</param>
    public static void LoadData(string filePath)
    {
        logger.Debug($"filePath = {filePath}");

        _hosts.Clear();

        foreach (var line in File.ReadLines(filePath))
        {
            var trimmed = line.Trim();

            // Skip comments and empty lines
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                continue;

            var parts = trimmed.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (!_hosts.ContainsKey(key))
                {
                    _hosts[key] = value;
                }
            }
        }
    }

    /// <summary>
    /// Loads XRF host data from an embedded resource into the internal dictionary <c>_hosts</c>.
    /// </summary>
    /// <remarks>
    /// The embedded resource is expected to be a text file (e.g., <c>DExtra_Hosts.txt</c>) located
    /// in the same namespace as the <c>XrfHosts</c> class and marked as <c>EmbeddedResource</c>
    /// in the project file. Lines starting with '#' or empty lines are ignored.
    /// Each valid line must contain a reflector designator and its corresponding IP address,
    /// separated by spaces or tabs. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the namespace of <see cref="DPlusHostRepository"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the embedded resource file cannot be found in the assembly.</exception>
    public static void LoadDataFromEmbeddedResource()
    {
        logger.Debug("");

        // Get the assembly that actually contains the resource
        var assembly = typeof(XrfHosts).Assembly;

        string? ns = typeof(XrfHosts).Namespace;

        if (string.IsNullOrEmpty(ns))
            throw new InvalidOperationException("Namespace of XrfHosts is null or empty.");

        string resourceName = $"{ns}.{fileName}";

        // Check available resources
        var names = assembly.GetManifestResourceNames();
      
        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Resource '{resourceName}' not found. Available: {string.Join(", ", names)}");
        using var reader = new StreamReader(stream);

        _hosts.Clear();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Trim();

            // Skip comments and empty lines
            if (line == null || string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (!_hosts.ContainsKey(key))
                {
                    _hosts[key] = value;
                }
            }
        }
    }

    /// <summary>
    /// Gets the IP address for a given reflector designator.
    /// </summary>
    /// <param name="designator">The reflector designator (e.g. "XRF001").</param>
    /// <returns>The IP address as a string, or <c>null</c> if not found.</returns>
    public static string? GetAddress(string designator)
    {
        if (string.IsNullOrWhiteSpace(designator))
            return null;

        return _hosts.TryGetValue(designator.Trim(), out var addr) ? addr : null;
    }

    /// <summary>
    /// Returns all known reflector mappings.
    /// </summary>
    public static IReadOnlyDictionary<string, string> All => _hosts;
}