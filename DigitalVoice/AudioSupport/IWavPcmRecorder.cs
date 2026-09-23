namespace DigitalVoice.AudioSupport;


/// <summary>
/// Abstraktion für einen PCM-Recorder, der PCM-Rohdaten in eine Datei schreibt (z.B. WAV
/// unter Windows via <c>WavPcmRecorder</c>/NAudio). Plattformneutral, damit Android
/// eine eigene, dateiformat- und API-passende Implementierung bereitstellen kann.
/// </summary>
public interface IWavPcmRecorder : IDisposable
{
    /// <summary>
    /// Schreibt PCM-Daten in die Zieldatei.
    /// </summary>
    /// <param name="pcmData">PCM-Byte-Array.</param>
    /// <param name="offset">Offset im Array, ab dem geschrieben werden soll.</param>
    /// <param name="count">Anzahl zu schreibender Bytes (Standard: Rest des Arrays ab <paramref name="offset"/>).</param>
    /// <exception cref="ObjectDisposedException">Der Recorder wurde bereits geschlossen/disposed.</exception>
    void WritePcm(byte[] pcmData, int offset = 0, int count = -1);

    /// <summary>
    /// Schreibt ausstehende Daten und schließt die Datei.
    /// </summary>
    void Close();
}
