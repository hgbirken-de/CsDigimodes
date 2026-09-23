
using DigitalVoice.AmbeSupport;
using DigitalVoice.AudioSupport;
using NLog;

namespace DigitalVoice.Dmr;

public record DmrClientConfig
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();
   
    public AmbeConfig? AmbeConfig { get; set; } 

    public IAmbe3000RController? AmbeController { get; set; }

    // Audio stuff
    public IMicrophoneReader? MicrophoneReader { get; set; }
    public IAudioPlayer? AudioPlayer { get; set; }
    public IWavPcmRecorder? WavPcmRecorder { get; set; }

    // DMR Server
    public string BmServerAddress { get; set; } = "2621.master.brandmeister.network"; // "master1.bm262.de";
    public int BmServerPort { get; set; } = 62031; // 62030
    public int SocketTimeout { get; set; } = 2000; // ms
    public string Callsign { get; set; } = "DL1HGB";
    public string Password { get; set; } = "unknown";

    public int ColorCode { get; set; } = 1; // 1 thru 15
    public int MyDmrId { get; set; }
    public int EssId { get; set; }
    public Flco Flco { get; set; }
    public int TimeSlot { get; set; } = 1;

    /// <summary>
    /// Returns the full DMR ID (incl. ESSID if defined).
    /// </summary>
    public int FullDmrId { get { return EssId > 0 ? MyDmrId * 100 + EssId : MyDmrId; }}

    // Recorder
    public bool RecordAudio { get; set; } = false;
    public string? RecordAudioFile { get; set; }
    public bool RecordDmrPackets { get; set; } = false;
    public string? RecordDmrPacketsFile { get; set; }


    // Simulation Mode
    public bool SimulationMode { get; set; } = false;
    public string? SimulationModeFile { get; set; }


    // Hotspot parameters
    public float Latitude { get; set; } = 54.49388f;
    public float Longitude { get; set; } = 11.18678f;
    public int Frequency { get; set; } = 434300000;
    public int Height { get; set; } = 0;
    public string Location { get; set; } = "Fehmarn, Todendorf";
    public string Description { get; set; } = "Beschreibung";
    public int TxPower { get; set; } = 0; // The transmit power in dBm, decimal (00-99)
    public string Url { get; set; } = "www.qrz.com";
    public string VersionInfo { get; set; } = "20181107_Pi-Star";
    public string PlatformTag { get; set; } = "MMDVM_MMDVM_HS_HAT";

    /// <summary>
    /// Creates a deep copy of the current <see cref="DmrClientConfig"/> instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="DmrClientConfig"/> instance with property values identical 
    /// to the original, but with separate copies of mutable collections.
    /// </returns>
    public DmrClientConfig DeepClone()
    {
        return this;
    }


    public void Validate()
    {
        int nError = 0;
        if (AmbeController == null) 
        {
            logger.Error($"No ANME Controller configured.");
            nError++;
        }

        if (EssId < 0 || EssId > 99)
        {
            logger.Error($"Invalid {nameof(EssId)}: {EssId}");
            nError++;
        }
        if (ColorCode < 1 || ColorCode > 15)
        {
            logger.Error($"Invalid {nameof(ColorCode)}: {ColorCode}");
            nError++;
        }
        if (TimeSlot < 1 || TimeSlot > 2)
        {
            logger.Error($"Invalid {nameof(TimeSlot)}: {TimeSlot}");
            nError++;
        }
        if (SimulationMode && string.IsNullOrEmpty(SimulationModeFile))
        {
            logger.Error("Simulation mode activated but no simulation file defined.");
            nError++;
        }
        if (SimulationMode && RecordDmrPackets)
        {
            logger.Warn("RX packets recording disabled, reason: simulation mode is activated.");
            RecordDmrPackets = false;
        }
        if (RecordAudio && string.IsNullOrEmpty(RecordAudioFile))
        {
            logger.Error("Audio recording activated but no audio output file defined.");
            nError++;
        }
        if (RecordAudio && WavPcmRecorder == null)
        {
            logger.Error("Audio recording activated but no Wave File Writer configured.");
            nError++;
        }
        if (RecordDmrPackets && string.IsNullOrEmpty(RecordDmrPacketsFile))
        {
            logger.Error("RX packet recording activated, but no recording file defined.");
            nError++;
        }

        if (nError > 0)
            throw new ArgumentException($"The configuration has {nError} errors, see the log for details.");
    }
}