using NLog;

namespace DigitalVoice.Fusion;

/// <summary>
/// Provides access to YSF reflector host information.
/// </summary>
/// <remarks>
/// This class maintains an internal dictionary of YSF hosts, which can be loaded either from an external file 
/// using <see cref="LoadData(string)"/> or from an embedded resource using <see cref="LoadDataFromEmbeddedResource"/>.  
/// Each host entry contains the following fields:  
/// <list type="bullet">
/// <item>Designator</item>
/// <item>Full name</item>
/// <item>Description</item>
/// <item>Host address</item>
/// <item>Port number</item>
/// <item>User count</item>
/// </list>
/// The class provides the methods <see cref="GetHostInfo(string)"/> and <see cref="TryGetHostInfo(string, out ValueTuple{string, string, string, int, string})"></see>"/> 
/// to retrieve host information by designator.
/// </remarks>
public static class YsfHosts
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    const string fileName = "YSF_Hosts.txt";

    public readonly static Dictionary<string, (string FullName, string Description, string Host, int Port, string UserCount)> _hosts = [];

    static YsfHosts()
    {
        LoadDataFromEmbeddedResource();
    }

    /// <summary>
    /// Retrieves detailed host information for a given reflector designator.
    /// </summary>
    /// <param name="designator">
    /// The reflector designator (e.g., "YSF001", "DCS001") to look up.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description><c>FullName</c> – The reflector's full display name.</description></item>
    /// <item><description><c>Description</c> – A human-readable description of the reflector.</description></item>
    /// <item><description><c>Host</c> – The host address (IP or DNS name).</description></item>
    /// <item><description><c>Port</c> – The network port number.</description></item>
    /// <item><description><c>UserCount</c> – The number of connected users as a string.</description></item>
    /// </list>
    /// If the designator is not found, <c>default</c> is returned, which corresponds to a tuple where all elements are <c>null</c> or <c>0</c>.
    /// </returns>
    public static (string FullName, string Description, string Host, int Port, string UserCount) GetHostInfo(string designator)
    {
        if (_hosts.TryGetValue(designator, out var info))
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
    public static bool TryGetHostInfo(string designator, out (string FullName, string Description, string Host, int Port, string UserCount) info)
    {
        if (_hosts.TryGetValue(designator, out info))
        {
            return true;
        }

        info = default;
        return false;
    }

    /// <summary>
    /// Loads YSF host data from a CSV-like file and populates the internal host dictionary.
    /// </summary>
    /// <param name="filePath">The full path to the host data file. Each line should contain 6 semicolon-separated fields:
    /// <c>Designator;FullName;Description;Host;Port;UserCount</c>. Lines starting with '#' or empty lines are ignored.
    /// </param>
    /// <remarks>
    /// The method clears any existing host data before loading new entries. 
    /// Invalid lines (e.g., missing fields or non-integer ports) are skipped with a warning.
    /// </remarks>
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

            var parts = trimmedLine.Split(';');
            if (parts.Length >= 6)
            {
                var designator = parts[0];
                var fullName = parts[1];
                var description = parts[2];
                var host = parts[3];

                if (!int.TryParse(parts[4], out var port))
                {
                    logger.Warn($"Invalid port value in line: {line}");
                    continue;
                }

                var userCount = parts[5];
                _hosts[designator] = (fullName, description, host, port, userCount);
            }
        }
    }

    /// <summary>
    /// Loads YSF host data from an embedded resource file and populates the internal host dictionary.
    /// </summary>
    /// <remarks>
    /// The method uses reflection to locate the embedded resource within the assembly where <see cref="YsfHosts"/> is defined.
    /// The resource name is constructed from the namespace of <see cref="YsfHosts"/> and the <c>fileName</c> constant.
    /// 
    /// The method clears any existing host data before loading new entries. 
    /// Lines starting with '#' or empty lines are ignored. Each line is expected to have 6 semicolon-separated fields:
    /// <c>Designator;FullName;Description;Host;Port;UserCount</c>.
    /// Lines with invalid port values are skipped, and a warning is logged.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the namespace of <see cref="YsfHosts"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the embedded resource cannot be found in the assembly.</exception>
    public static void LoadDataFromEmbeddedResource()
    {
        logger.Debug("");

        // Get the assembly that actually contains the resource
        var assembly = typeof(YsfHosts).Assembly;

        string? ns = typeof(YsfHosts).Namespace;

        if (string.IsNullOrEmpty(ns))
            throw new InvalidOperationException("Namespace of YsfHosts is null or empty.");

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

            var parts = line.Split(';');
            if (parts.Length >= 6)
            {
                var designator = parts[0];
                var fullName = parts[1];
                var description = parts[2];
                var host = parts[3];

                if (!int.TryParse(parts[4], out var port))
                {
                    logger.Warn($"Invalid port value in line: {line}");
                    continue;
                }

                var userCount = parts[5];
                _hosts[designator] = (fullName, description, host, port, userCount);
            }
        }
    }

    /// <summary>
    /// Returns all known YSF host mappings.
    /// </summary>
    public static IReadOnlyDictionary<string, (string FullName, string Description, string Host, int Port, string UserCount)> All => _hosts;
}

