using NLog;
using System.Runtime.InteropServices;

using YamlDotNet.Serialization;

namespace YsfReflector;

/// <summary>
/// Class to read/write and manage user specific settings.
/// </summary>
public class Config
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static Config? _instance;

    public static string FileName { get; private set; } // file with yaml data to be read

    //public static readonly string appDir = "DigitalVoiceControl"; // <- must be defined here and only here!!!
	public static readonly string appName = "DigitalVoiceControl"; // <- must be defined here and only here!!!

    public static readonly bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static readonly bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public ClientMgmt ClientMgmt { get; private set; } = new();
    public DgIdMgmt DgIdMgmt { get; private set; } = new();
    public Protections Protections { get; private set; } = new();
    public Info Info { get; private set; } = new();
    public Network Network { get; private set; } = new();

    /// <summary>
    /// Static initializer that creates the app specific folder structure if not existing.
    /// </summary>
    static Config()
    {
        FileName = typeof(Config).Name + ".yaml";
        logger.Debug("");
    }

    /// <summary>
    /// Returms a singleton instance.
    /// </summary>
    /// <returns>the singleton instance</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Config Instance()
    {
        // Get the assembly that actually contains the resource
        var assembly = typeof(Reflector).Assembly;

        string? ns = typeof(Reflector).Namespace;

        if (string.IsNullOrEmpty(ns))
            throw new InvalidOperationException("Namespace of DmrHosts is null or empty.");

        string resourceName = $"{ns}.{FileName}";

        // Check available resources
        var names = assembly.GetManifestResourceNames();

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Resource '{resourceName}' not found. Available: {string.Join(", ", names)}");
        using var reader = new StreamReader(stream);

        if (_instance == null)
        {
            try
            {
                string yamlText = reader.ReadToEnd();
                var deserializer = new DeserializerBuilder().Build();
                _instance = deserializer.Deserialize<Config>(yamlText);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            _instance ??= new Config(); // something went wrong, asure that we have values
        }
        return _instance;
    }

    
    /// <summary>
    /// Save the current UserSettings to disk file in Yaml format.
    /// </summary>
    public static void Save()
    {
        if (_instance == null) return; // nothing to save
        var serializer = new SerializerBuilder().Build();
        string yamlText = serializer.Serialize(_instance);
        File.WriteAllText(FileName, yamlText);
    }
}

public class ClientMgmt
{
    public string AccessControlFile { get; set; } = "./db/access_control.txt";
    public int CheckRE { get; set; } = 0;
    public int InactivityTimeoutSeconds { get; set; } = 15;
    public int MaxInactivityPeriodSeconds { get; set; } = 60;
    public int MaxClients { get; set; } = 50;
}

public class DgIdMgmt
{
    public List<int> DgIdList = [1, 9, 50];
    public int DgIdDefault { get; set; } = 50;
    public int DgIdLocal { get; set; } = 1;
    public int Prefix { get; set; } = 0;
    public string HomeDgIdFile { get; set; } = "./db/home_dgids.csv";
}

public class Info
{
    public string Description { get; set; } = "YSF Reflector";
    public string Name { get; set; } = "YsfReflector";
    public string ReflectorId { get; set; } = "00000";
    public string Version { get; set; } = "1.0.0";
}

public class Network
{
    public string IpAddr { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 42000;
}

public class Protections
{
    public int Reactivate { get; set; } = 900;
    public int Timeout { get; set; } = 180;
    public int WildPttCount { get; set; } = 4;
    public int WildPttTime { get; set; } = 5;
}
