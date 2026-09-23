using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using YamlDotNet.Serialization;

namespace DigitalVoiceControlApp.Config;

/// <summary>
/// Class to read/write and manage user specific settings.
/// </summary>
public class UserSettings
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static UserSettings? _instance;

    public static string FileName { get; private set; } // file with yaml data to be read

    //public static readonly string appDir = "DigitalVoiceControl"; // <- must be defined here and only here!!!
	public static readonly string appName = "DigitalVoiceControl"; // <- must be defined here and only here!!!

    public static readonly string homeDir;

    public static readonly bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static readonly bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public enum FileType { Audio, Data }

    static readonly Dictionary<FileType, string> appSpecificFolder = new() { [FileType.Audio] = "audio", [FileType.Data] = "Data" };

    public Ambe Ambe { get; private set; } = new();

    public Common Common { get; private set; } = new();

    public Dcs Dcs { get; private set; } = new();

    public Dmr Dmr { get; private set; } = new();

    public Fcs Fcs { get; private set; } = new();

    public Gui Gui { get; private set; } = new();

    public Hotspot Hotspot { get; private set; } = new();

    public Nxdn Nxdn { get; private set; } = new();

    public Ref Ref { get; private set; } = new();

    public Xrf Xrf { get; private set; } = new();
    public Ysf Ysf { get; private set; } = new();

    /// <summary>
    /// Static initializer that creates the app specific folder structure if not existing.
    /// </summary>
    static UserSettings()
    {
        string baseDir = isLinux
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share")
                : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        homeDir = Path.Combine(baseDir, appName);
        //homeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        Directory.CreateDirectory(homeDir);

        Directory.CreateDirectory(Path.Combine(homeDir, "logs")); // logging goes here

        // Create App specific folders (if not existing)
        foreach (Mode item in Enum.GetValues(typeof(Mode)))
        {
            string txDir = Path.Combine(homeDir, item.ToString());
            Directory.CreateDirectory(txDir);
            //appSpecificFolder.Values.ToList().ForEach(s => Directory.CreateDirectory(Path.Combine(txDir, s)));
            foreach (var s in appSpecificFolder.Values)
            {
                Directory.CreateDirectory(Path.Combine(txDir, s));
            }
        }

        FileName = Path.Combine(homeDir, typeof(UserSettings).Name) + ".yaml";

        // Re-configure NLog (change log file)
        var config = LogManager.Configuration;
        if (config != null)
        {
            var fileTarget = config.FindTargetByName<FileTarget>("file"); // Get the target named "file"
            if (fileTarget != null)
            {
                string logFileName = Path.Combine(homeDir, "logs", $"log_{DateTime.UtcNow:yyyy-MM-ddTHHmmss}.log");
                fileTarget.FileName = logFileName;
                LogManager.ReconfigExistingLoggers(); // Important: re-apply the updated config
            }
        }

        logger.Debug("");
    }

    /// <summary>
    /// Returms a singleton instance.
    /// </summary>
    /// <returns>the singleton instance</returns>
    /// <exception cref="ArgumentException"></exception>
    public static UserSettings Instance()
    {
        if (_instance == null)
        {
            try
            {
                string yamlText = File.ReadAllText(FileName);
                var deserializer = new DeserializerBuilder().Build();
                _instance = deserializer.Deserialize<UserSettings>(yamlText);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            _instance ??= new UserSettings(); // something went wrong, asure that we have values
        }
        return _instance;
    }

    /// <summary>
    /// Returns the TX specific directory of a specific file type.
    /// </summary>
    /// <param name="mode">Defines the mode.</param>
    /// <param name="fileType">Defines the file type <see cref="FileType"/></param>
    /// <returns></returns>
    public static string Dir(Mode mode, FileType fileType)
    {
        return Path.Combine(homeDir, mode.ToString(), appSpecificFolder[fileType]);
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

public class Ambe
{
    public string ServerAddr { get; set; } = "127.0.0.1";
    public int ServerPort { get; set; } = 2460;
    public AmbeServiceType ServiceType { get; set; }
    public int StickBaudrate { get; set; } = 460800;
    public string StickPort { get; set; } = "COM9";
}

public class Common
{
    public string Callsign { get; set; } = "NOCALL";
    public string Language { get; set; } = "en-US";
    public Mode LastMode { get; set; } = Mode.Fcs;
    public string Locator { get; set; } = "JO54OL";
    public double MicGain { get; set; } = 50;
    public string Name { get; set; } = "Unknown";
    public bool RecordRcvdUdpPackets { get; set; } = false;
    public double RxVolume { get; set; } = 50;
    public bool TextToSpeech { get; set; } = false;
    public string Town { get; set; } = "Unknown";
}

public class Dcs
{
    public string LastReflector { get; set; } = "DCS001";
    public char LastModule { get; set; } = 'C';
    public int HostPort { get; set; } = 30051;
    public string UserMessage { get; set; } = "DVC by DL1HGB";
}

public class Dmr
{
    public int MyDmrId { get; set; } = 2622363;
    public int Essid { get; set; } = 15;
    public string Password { get; set; } = "unknown";
    public DmrProtocol Protocol { get; set; } = DmrProtocol.MmdvmHost;
    public int LastTgInUse { get; set; } = 262997;
    public string BmServerAddr1 { get; set; } = "master1.bm262.de"; // for Homebrew
    public int BmServerPort1 { get; set; } = 62030; // for Homebrew
    public string Master { get; set; } = "BM_2621_Germany";

    public int TimeSlot { get; set; } = 1;

    public int ColorCode { get; set; } = 1;
}

public class Fcs
{
    public string Master { get; set; } = "FCS001 (DE)";
    public int Port { get; set; } = 62500;
    public string LastReflector { get; set; } = "FCS00199";
}

public class Gui
{
    public bool Resizable { get; set; } = true;
    public int FrameXPos { get; set; } = 10;
    public int FrameYPos { get; set; } = 10;
    public double FrameHeight { get; set; } = 0;
    public double FrameWidth { get; set; } = 0;
}

public class Hotspot
{
    public int RxFrequency { get; set; } = 434300000;
    public int TxFrequency { get; set; } = 434300000;
    public string Type { get; set; } = "DVC";
}

public class Nxdn
{
    public int NxdnId { get; set; } = 39251;
    public int LastReflectorId { get; set; } = 20000;
}

public class Ref
{
    public string LastReflector { get; set; } = "REF000";
    public char LastModule { get; set; } = 'A';
    public int HostPort { get; set; } = 20001;
    public string UserMessage { get; set; } = "DVC by DL1HGB";
}

public class Xrf
{
    public string LastReflector { get; set; } = "XRF002";
    public char LastModule { get; set; } = 'C';
    public int HostPort { get; set; } = 30001;
    public string UserMessage { get; set; } = "DVC by DL1HGB";
}

public class Ysf
{
    public string LastReflector { get; set; } = "99999";
}