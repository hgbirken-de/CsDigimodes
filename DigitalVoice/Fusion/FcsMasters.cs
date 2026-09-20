
using NLog;

namespace DigitalVoice.Fusion;

/// <summary>
/// Provides access to FCS master information.
/// </summary>
/// <remarks>
/// This class maintains an internal dictionary of FCS masters, which can be loaded either from an external file 
/// using <see cref="LoadData(string)"/> or from an embedded resource using <see cref="LoadDataFromEmbeddedResource"/>.  
/// Each host entry contains the following fields:  
/// <list type="number">
/// <item>name</item>
/// <item>ip-address</item>
/// </list>
/// The class provides the methods <see cref="GetMasterAddr(string)"/> and <see cref="TryGetMasterAddr(string, out ValueTuple{string, string})"></see>"/> 
/// to retrieve host information by designator.
/// </remarks>
public static class FcsMasters
{
    const string fileName = "FCSMasters.csv";

    static FcsMasters()
    {
        LoadDataFromEmbeddedResource();
    }

    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static readonly Dictionary<string, string> _hosts = [];


    /// <summary>
    /// Retrieves detailed host information for a given reflector designator.
    /// </summary>
    /// <param name="name">The name of the reflector to look up.
    /// </param>
    /// <returns>The masters ip address or null if name not found.</returns>
    public static string? GetMasterAddr(string name) => _hosts.TryGetValue(name, out var info) ? info : null;


    /// <summary>
    /// Tries to retreve the ip address of a FCS master given its name.
    /// </summary>
    /// <param name="name">The FCS master's name.</param>
    /// <param name="masterAddr">The FCS master's ip address.</param>
    /// <returns>true if lookup successful, otherwise false</returns>
    public static bool TryGetMasterAddr(string name, out string? masterAddr) => _hosts.TryGetValue(name, out masterAddr);


    /// <summary>
    /// Loads FCS master data from a CSV-like file and populates the internal master dictionary.
    /// </summary>
    /// <param name="filePath">The full path to the FCS master data file. Each line should contain 2 semicolon-separated fields: <c>name;ip-address</c>. Lines starting with '#' or empty lines are ignored.
    /// </param>
    /// <remarks>
    /// The method clears any existing master data before loading new entries. 
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
            if (parts.Length >= 2)
            {
                var name = parts[0];
                var addr = parts[1];
                _hosts[name] = addr;
            }
        }
    }

    /// <summary>
    /// Loads FCS host data from an embedded resource file and populates the internal host dictionary.
    /// </summary>
    /// <remarks>
    /// The method uses reflection to locate the embedded resource within the assembly where <see cref="FcsHosts"/> is defined.
    /// The resource name is constructed from the namespace of <see cref="FcsHosts"/> and the <c>fileName</c> constant.
    /// 
    /// The method clears any existing host data before loading new entries.
    /// Lines starting with '#' or empty lines are ignored. Each line is expected to have 6 comma-separated fields: <c>Designator,FullName,Description</c>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the namespace of <see cref="YsfHosts"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the embedded resource cannot be found in the assembly.</exception>
    public static void LoadDataFromEmbeddedResource()
    {
        logger.Debug("");

        // Get the assembly that actually contains the resource
        var assembly = typeof(FcsMasters).Assembly;

        string? ns = typeof(FcsMasters).Namespace;

        if (string.IsNullOrEmpty(ns))
            throw new InvalidOperationException("Namespace of FcsMasters is null or empty.");

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
            if (parts.Length >= 2)
            {
                var name = parts[0];
                var addr = parts[1];
                _hosts[name] = addr;
            }
        }
    }

    /// <summary>
    /// Returns all known FCS master mappings.
    /// </summary>
    public static IReadOnlyDictionary<string, string> All => _hosts;
}