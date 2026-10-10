namespace DigitalVoice.SoftwareVocoder;

/// <summary>
/// Ergebnis der Sprachanalyse eines 20-ms-Frames, so wie der AMBE-Quantisierer es braucht (die vier Felder des
/// <c>IMBE_PARAM</c> des C-Originals, die <c>encode_ambe</c> liest).
/// </summary>
public sealed class ImbeParameters
{
    /// <summary>Verfeinerte Tonhöhe (Q8.8), <c>ref_pitch</c>.</summary>
    public short RefPitch;

    /// <summary>Anzahl der Harmonischen (9 bis 56), <c>num_harms</c>.</summary>
    public short NumHarms;

    /// <summary>Spektralamplituden je Harmonischer, <c>sa</c>.</summary>
    public readonly short[] Sa = new short[56];

    /// <summary>Stimmhaft (1) oder stimmlos (0) je Harmonischer, <c>v_uv_dsn</c>.</summary>
    public readonly short[] VUvDsn = new short[56];
}

/// <summary>
/// Sprachanalyse für den Software-Kodierer: bestimmt aus 160 PCM-Samples Tonhöhe, Stimmhaft/Stimmlos-Entscheidung und
/// Spektralamplituden. Die Implementierung <c>ImbeAnalyzer</c> steht im eigenen Projekt
/// <c>DigitalVoice.SoftwareVocoder.Imbe</c> (GPL), damit der Rest von DigitalVoice davon unabhängig bleibt.
/// </summary>
public interface IImbeAnalyzer
{
    /// <summary>Analysiert den nächsten Frame (160 Samples, 8 kHz, 16 Bit). Die Analyse hat Gedächtnis über die Frames.</summary>
    void Analyze(ReadOnlySpan<short> pcm, ImbeParameters result);

    /// <summary>Setzt das Gedächtnis auf den Anfang eines neuen Datenstroms zurück.</summary>
    void Reset();
}
