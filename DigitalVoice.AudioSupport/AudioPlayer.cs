using NAudio.Wave;
using System.Runtime.InteropServices;

namespace DigitalVoice.AudioSupport;

/// <summary>
/// Streams and plays real-time PCM audio using NAudio.
/// </summary>
/// <remarks>
/// Designed for scenarios where raw PCM audio is produced incrementally 
/// (e.g., by a decoder, vocoder, or network source). The class buffers 
/// the incoming audio and ensures smooth playback with low latency.
/// 
/// <para>Expected format:</para>
/// <list type="bullet">
///   <item><description>Signed 16-bit PCM, little endian</description></item>
///   <item><description>Customizable sample rate (e.g., 8000 Hz)</description></item>
///   <item><description>Mono or stereo channels</description></item>
/// </list>
/// </remarks>
public class AudioPlayer : IAudioPlayer, IDisposable
{
    private readonly WaveOutEvent _waveOut;
    private readonly BufferedWaveProvider _buffer;
    private bool _disposed;

    // Gain in dB (default 0 = unity)
    private float _gainDb = 0.0f;

    private float _gain = 1.0f;

    public float GainDb
    {
        get => _gainDb;
        set { _gainDb = value; _gain = (float)Math.Pow(10.0, value / 20.0); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
    /// </summary>
    /// <param name="sampleRate">The audio sample rate in Hz (default: 8000).</param>
    /// <param name="channels">The number of audio channels (1 = mono, 2 = stereo).</param>
    /// <remarks>
    /// The player automatically creates a <see cref="BufferedWaveProvider"/> 
    /// with a 1-second buffer length, and starts playback immediately.
    /// </remarks>
    public AudioPlayer(int sampleRate = 8000, int channels = 1)
    {
        var format = new WaveFormat(sampleRate, 16, channels);
        _buffer = new BufferedWaveProvider(format)
        {
            DiscardOnBufferOverflow = true,
            BufferLength = sampleRate * 2 * channels
        };

        _waveOut = new WaveOutEvent();
        _waveOut.Init(_buffer);
        _waveOut.Play();
    }

    /// <summary>
    /// Feeds PCM audio samples into the playback buffer.
    /// </summary>
    /// <param name="pcmData">A byte array containing raw PCM data in 16-bit little-endian format.</param>
    /// <param name="offset">The zero-based index in <paramref name="pcmData"/> at which to begin copying.</param>
    /// <param name="count">The number of bytes to copy. If set to <c>-1</c> (default), the entire buffer starting at <paramref name="offset"/> is used.</param>
    /// <remarks>
    /// The method writes data into a <see cref="BufferedWaveProvider"/>.  
    /// If the buffer is full, older audio samples may be dropped 
    /// when <see cref="BufferedWaveProvider.DiscardOnBufferOverflow"/> is <c>true</c>.
    /// </remarks>
    public void FeedPcmData(byte[] pcmData, int offset = 0, int count = -1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (pcmData == null || pcmData.Length == 0)
            return;

        if (count == -1)
            count = pcmData.Length;

        // Slice the working window
        Span<byte> pcmSlice = pcmData.AsSpan(offset, count);

        // Reinterpret bytes as little-endian 16-bit PCM samples
        Span<short> samples = MemoryMarshal.Cast<byte, short>(pcmSlice);

        for (int i = 0; i < samples.Length; i++)
        {
            int scaled = (int)(samples[i] * _gain);

            // Saturation clamp
            if (scaled > short.MaxValue) scaled = short.MaxValue;
            else if (scaled < short.MinValue) scaled = short.MinValue;

            samples[i] = (short)scaled;
        }

        _buffer.AddSamples(pcmData, offset, count);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Core dispose logic.
    /// </summary>
    /// <param name="disposing">True if called explicitly, false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // free managed resources
            _waveOut?.Stop();
            _waveOut?.Dispose();
            // _buffer does not need explicit disposal (no unmanaged resources)
        }

        _disposed = true;
    }
}
