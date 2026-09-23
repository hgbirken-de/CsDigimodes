
using DigitalVoice.AmbeSupport;
using DigitalVoice.AudioSupport;
using NLog;

namespace DigitalVoice.Fusion;

public record FcsClientConfig
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    string _callsign = string.Empty;
    string _reflectorId = "FCS00137";

    // YSF Reflector
    public string ReflectorAddress { get; set; } = "fcs001.xreflector.net";
    public int ReflectorPort { get; set; } = 62500;
    public string ReflectorId
    {
        get => _reflectorId;
        set
        {
            if (string.IsNullOrEmpty(value) || value.Length != 8)
                throw new ArgumentException($"Invalid reflectorId, must be a string of 8 chars.");
            _reflectorId = value.ToUpper();
        }
    }

    // AMBE stuff
    public string AmbeServerAddr { get; set; } = "127.0.0.1";
    public int AmbeServerPort { get; set; } = 2460;
    public int AmbeStickBaudrate { get; set; } = 460800;
    public string AmbeStickComport { get; set; } = "COM13";
    public AmbeServiceType AmbeServiceType { get; set; } = AmbeServiceType.Stick;
    public IAmbe3000RController? AmbeController { get; set; }


    // Audio stuff
    public IMicrophoneReader? MicrophoneReader { get; set; }
    public IAudioPlayer? AudioPlayer { get; set; }
    public IWavPcmRecorder? WavPcmRecorder { get; set; }

    // Simulation Mode
    public string? SimulationFile { get; set; }
    public bool SimulationMode { get; set; }

    // Recorder
    public bool RecordAudio { get; set; } = false;
    public string? RecordAudioFile { get; set; }
    public bool RecordFcsPackets { get; set; } = false;
    public string? RecordFcsPacketsFile { get; set; }


    // DGIDs
    public List<int> DgidList { get; set; } = [1, 37];

    // General
    public string Callsign { get => _callsign;
        set 
        {
            if (string.IsNullOrEmpty(value) || value.Length > 6)
                throw new ArgumentException($"Invalid callsign, must be a string of max. 6 chars.");
            _callsign = value.ToUpper();
        }
    }

    public string HotspotType { get; set; } = "MMDVM";
    
    public string Locator { get; set; } = string.Empty;
    
    public string Town { get; set; } = string.Empty;

    public int RxFrequency { get; set; } = 434300000;
    public int TxFrequency { get; set; } = 434300000;

    // Info
    public string? Name { get; set; }
    public string? Description { get; set; }


    /// <summary>
    /// Validates the current configuration state and enforces consistency rules.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when one or more errors are detected in the configuration.</exception>
    public void Validate()
    {
        int errorCount = 0;
        if (AudioPlayer == null)
        {
            logger.Error("Missing AudioPlayer.");
            errorCount++;
        }
        if (SimulationMode && string.IsNullOrEmpty(SimulationFile))
        {
            logger.Error("Simulation mode activated but no simulation file defined.");
            errorCount++;
        }
        if (SimulationMode && RecordFcsPackets)
        {
            logger.Warn("Ysf packet recording disabled, reason: simulation mode is activated.");
            RecordFcsPackets = false;
        }
        if (RecordAudio && string.IsNullOrEmpty(RecordAudioFile))
        {
            logger.Error("Audio recording activated but no audio output file defined.");
            errorCount++;
        }
        if (RecordFcsPackets && string.IsNullOrEmpty(RecordFcsPacketsFile))
        {
            logger.Error("Ysf packet recording activated, but no recording file defined.");
            errorCount++;
        }

        if (errorCount > 0)
            throw new ArgumentException($"The configuration has {errorCount} errors, see the log for details.");
    }

    /// <summary>
    /// Creates a deep copy of the current <see cref="FcsClientConfig"/> instance.
    /// </summary>
    /// <remarks>
    /// This method performs a deep clone for mutable reference types such as 
    /// <see cref="DgidList"/> so that modifications in the cloned instance do not 
    /// affect the original.
    /// All value types and immutable types (e.g., <see cref="string"/>) are copied directly.
    /// </remarks>
    /// <returns>
    /// A new <see cref="FcsClientConfig"/> instance with property values identical 
    /// to the original, but with separate copies of mutable collections.
    /// </returns>
    public FcsClientConfig DeepClone()
    {
        return this with
        {
            DgidList = [.. this.DgidList]
        };
    }
}
