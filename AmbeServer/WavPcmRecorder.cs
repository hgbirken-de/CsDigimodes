using NAudio.Wave;

namespace AmbeServer;

/// <summary>
/// PCM recorder that writes PCM data to a WAV file using NAudio's WaveFileWriter.
/// </summary>
public class WavPcmRecorder : IDisposable
{
    private WaveFileWriter? _writer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavPcmRecorder"/> class.
    /// </summary>
    /// <param name="filePath">The path of the WAV file to write.</param>
    /// <param name="sampleRate">Sample rate in Hz (default: 8000).</param>
    public WavPcmRecorder(string filePath, int sampleRate = 8000)
    {
        var format = new WaveFormat(sampleRate, 16, 1); // 16-bit mono
        _writer = new WaveFileWriter(filePath, format);
    }

    /// <summary>
    /// Writes PCM data to the WAV file.
    /// </summary>
    /// <param name="pcmData">PCM byte array.</param>
    /// <param name="offset">Offset in the array to start writing from.</param>
    /// <param name="count">Number of bytes to write (default: rest of array).</param>
    public void WritePcm(byte[] pcmData, int offset = 0, int count = -1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (count == -1)
            count = pcmData.Length - offset;

        _writer!.Write(pcmData, offset, count);
    }

    /// <summary>
    /// Flushes and closes the WAV file.
    /// </summary>
    public void Close()
    {
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose pattern implementation.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Release managed resources
            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;
        }

        _disposed = true;
    }
}
