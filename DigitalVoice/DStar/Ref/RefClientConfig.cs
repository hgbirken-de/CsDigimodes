using DigitalVoice.AudioSupport;

namespace DigitalVoice.DStar.Ref;

public record RefClientConfig
{
    public string RefAddress { get; set; } = string.Empty;

    public int RefPort { get; set; } = 0;

    public string RefName { get; set; } = string.Empty;

    char _module = 'C';
    public char Module { get => _module; set { _module = char.ToUpper(value, System.Globalization.CultureInfo.InvariantCulture); }}

    public string Callsign { get; set; } = string.Empty;

    public MicrophoneReader? MicrophoneReader { get; set; }

    public AudioPlayer? AudioPlayer { get; set; }

    public bool RecordAudio { get; set; } = false;

    public bool RecordRefPackets { get; set; } = false;

    public string? SimulationFile { get; set; }
    public bool SimulationMode { get; set; } = false;

}