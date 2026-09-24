using DigitalVoice.AmbeSupport;
using DigitalVoice.AudioSupport;

namespace DigitalVoice.DStar.Xrf;

public record XrfClientConfig
{
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

    public bool RecordRefPackets { get; set; } = false;

    public string? SimulationFile { get; set; }
    public bool SimulationMode { get; set; } = false;

    public string UserMessage { get; set; } = "DVC by DL1HGB";

}