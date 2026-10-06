using DigitalVoice.AmbeSupport;
using DigitalVoice.AudioSupport;
using NLog;

namespace DigitalVoice.DStar.Dcs;

public record DcsClientConfig
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // AMBE stuff
    public AmbeConfig? AmbeConfig { get; set; }
    public IAmbe3000RController? AmbeController { get; set; }

    // Audio stuff
    public IMicrophoneReader? MicrophoneReader { get; set; }
    public IAudioPlayer? AudioPlayer { get; set; }
    public IWavPcmRecorder? WavPcmRecorder { get; set; }


    public string RefAddress { get; set; } = string.Empty;

    public int RefPort { get; set; } = 0;

    public string RefName { get; set; } = string.Empty;

    char _module = 'C';
    public char Module { get => _module; set { _module = char.ToUpper(value, System.Globalization.CultureInfo.InvariantCulture); }}

    public string Callsign { get; set; } = string.Empty;

    public bool RecordAudio { get; set; } = false;

    public bool RecordRcvdUdpPackets { get; set; } = false;

    public string? RecordRcvdUdpPacketsFile { get; set; }

    public string? SimulationFile { get; set; }
    public bool SimulationMode { get; set; } = false;

    public string UserMessage { get; set; } = "CsDigimodes";

    public void Validate()
    {
        int errCnt = 0;
        if (AmbeController == null)
        {
            logger.Error($"No ANME Controller configured.");
            errCnt++;
        }

        if (string.IsNullOrEmpty(Callsign))
        {
            logger.Error("No Callsign defined.");
            errCnt++;
        }

        if (SimulationMode && string.IsNullOrEmpty(SimulationFile))
        {
            logger.Error("Simulation mode activated but no simulation file defined.");
            errCnt++;
        }
        if (SimulationMode && RecordRcvdUdpPackets)
        {
            logger.Warn("RX packets recording disabled, reason: simulation mode is activated.");
            RecordRcvdUdpPackets = false;
        }
        //if (RecordAudio && string.IsNullOrEmpty(RecordAudioFile))
        //{
        //    logger.Error("Audio recording activated but no audio output file defined.");
        //    nError++;
        //}
        if (RecordAudio && WavPcmRecorder == null)
        {
            logger.Error("Audio recording activated but no Wave File Writer configured.");
            errCnt++;
        }
        if (RecordRcvdUdpPackets && string.IsNullOrEmpty(RecordRcvdUdpPacketsFile))
        {
            logger.Error("RX packet recording activated, but no recording file defined.");
            errCnt++;
        }

        if (errCnt > 0)
            throw new ArgumentException($"The configuration has {errCnt} errors, see the log for details.");
    }

}