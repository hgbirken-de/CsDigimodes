
using DigitalVoice.AudioSupport;
using NLog;

namespace DigitalVoice.Fusion;

public record YsfClientConfig
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // YSF Reflector
    public string ReflectorAddress { get; set; } = string.Empty;
    public int ReflectorPort { get; set; }

    // AMBE
    public bool AmbeSrvUsage { get; set; }
    public string AmbeSrvAddr { get; set; } = "127.0.0.1";
    public int AmbeSrvPort { get; set; } = 2460;

    // Simulation Mode
    public string? SimulationFile { get; set; }
    public bool SimulationMode { get; set; }

    // Recorder
    public bool RecordAudio { get; set; } = false;
    public string? RecordAudioFile { get; set; }
    public bool RecordYsfPackets { get; set; } = false;
    public string? RecordYsfPacketsFile { get; set; }

    // DGID
    public List<int> DgidList { get; set; } = [0, 1, 9, 50];

    // General
    public string Callsign { get; set; } = string.Empty;
    public string HotspotType { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string Locator { get; set; } = string.Empty;
    
    public int RxFrequency { get; set; } = 434300000;
    public int TxFrequency { get; set; } = 434300000;

    // Info
    public string? Name { get; set; }
    public string? Description { get; set; }

    // Audio stuff
    public MicrophoneReader? MicrophoneReader { get; set; }

    public AudioPlayer? AudioPlayer { get; set; }

    /// <summary>
    /// Validates the current configuration state and enforces consistency rules.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when one or more errors are detected in the configuration.</exception>
    public void Validate()
    {
        int nError = 0;
        if (SimulationMode && string.IsNullOrEmpty(SimulationFile))
        {
            logger.Error("Simulation mode activated but no simulation file defined.");
            nError++;
        }
        if (SimulationMode && RecordYsfPackets)
        {
            logger.Warn("Ysf packet recording disabled, reason: simulation mode is activated.");
            RecordYsfPackets = false;
        }
        if (RecordAudio && string.IsNullOrEmpty(RecordAudioFile))
        {
            logger.Error("Audio recording activated but no audio output file defined.");
            nError++;
        }
        if (RecordYsfPackets && string.IsNullOrEmpty(RecordYsfPacketsFile))
        {
            logger.Error("Ysf packet recording activated, but no recording file defined.");
            nError++;
        }

        if (nError > 0)
            throw new ArgumentException($"The configuration has {nError} errors, see the log for details.");
    }

    /// <summary>
    /// Creates a deep copy of the current <see cref="YsfClientConfig"/> instance.
    /// </summary>
    /// <remarks>
    /// This method performs a deep clone for mutable reference types such as 
    /// <see cref="DgidList"/> so that modifications in the cloned instance do not 
    /// affect the original.
    /// All value types and immutable types (e.g., <see cref="string"/>) are copied directly.
    /// </remarks>
    /// <returns>
    /// A new <see cref="YsfClientConfig"/> instance with property values identical 
    /// to the original, but with separate copies of mutable collections.
    /// </returns>
    public YsfClientConfig DeepClone()
    {
        return this with
        {
            DgidList = [.. this.DgidList]
        };
    }
}
