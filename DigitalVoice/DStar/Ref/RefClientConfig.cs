using DigitalVoice.AmbeSupport;
using DigitalVoice.AudioSupport;

namespace DigitalVoice.DStar.Ref;

public record RefClientConfig
{

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

}