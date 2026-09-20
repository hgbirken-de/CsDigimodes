using DigitalVoice.AudioSupport;
using NLog;

namespace DigitalVoice.Nxdn;

public class NxdnClientConfig
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public string AmbeSrvAddr { get; set; } = "127.0.0.1";
    public int AmbeSrvPort { get; set; } = 2460;

    public string NxdnReflectorAddr { get; set; } = "";
    public int NxdnReflectorPort { get; set; } = 0;
    public int NxdnReflectorId { get; set; } = 0;

    public string Callsign { get; set; } = "N0CALL";

    public int MyNxdnId { get; set; } = 39251;

    // Recorder
    public bool RecordAudio { get; set; } = false;
    public string? RecordAudioFile { get; set; }
    public bool RecordRxPackets { get; set; } = false;
    public string? RecordRxPacketsFile { get; set; }


    // Simulation Mode
    public bool SimulationMode { get; set; } = false;
    public string? SimulationModeFile { get; set; }


    // Audio stuff
    public MicrophoneReader? MicrophoneReader { get; set; }
    public AudioPlayer? AudioPlayer { get; set; }


    public void Validate()
    {
        int nError = 0;
        if (string.IsNullOrEmpty(Callsign) || Callsign.Length > 10)
        {
            logger.Error("Missing/Invalid callsign.");
            nError++;
        }
        if (SimulationMode && string.IsNullOrEmpty(SimulationModeFile))
        {
            logger.Error("Simulation mode activated but no simulation file defined.");
            nError++;
        }
        if (SimulationMode && RecordRxPackets)
        {
            logger.Warn("RX packets recording disabled, reason: simulation mode is activated.");
            RecordRxPackets = false;
        }
        if (RecordAudio && string.IsNullOrEmpty(RecordAudioFile))
        {
            logger.Error("Audio recording activated but no audio output file defined.");
            nError++;
        }
        if (RecordRxPackets && string.IsNullOrEmpty(RecordRxPacketsFile))
        {
            logger.Error("RX packet recording activated, but no recording file defined.");
            nError++;
        }

        if (nError > 0)
            throw new ArgumentException($"The configuration has {nError} errors, see the log for details.");

    }
}
