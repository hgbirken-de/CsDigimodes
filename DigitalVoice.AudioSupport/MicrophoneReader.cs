using NAudio.Wave;
using NLog;
using System.Runtime.InteropServices;

namespace DigitalVoice.AudioSupport; 

public sealed class MicrophoneReader(int sampleRate = 8000, int channels = 1) : IMicrophoneReader, IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private WaveInEvent? _waveIn;
    private BufferedWaveProvider? _bufferedWaveProvider;

    public WaveFormat Format { get; } = new WaveFormat(sampleRate, 16, channels);

    public event EventHandler<byte[]>? AudioAvailable;

    private float _gain = 1.0f;

    // Gain in dB (default 0 = unity)
    private float _gainDb = 0.0f;

    public float GainDb
    {
        get => _gainDb;
        set { _gainDb = value; _gain = (float)Math.Pow(10.0, value / 20.0); }
    }

    public void Start()
    {
        if (_waveIn != null)
            return; // already running

        _waveIn = new WaveInEvent
        {
            DeviceNumber = 0,   // default input
            WaveFormat = Format
        };

        _bufferedWaveProvider = new(Format)
        {
            DiscardOnBufferOverflow = true,
            BufferLength = Format.AverageBytesPerSecond/2, // ~500ms buffer
        };

        _waveIn.DataAvailable += (s, e) =>
        {
            // Forward PCM data to event subscribers
            AudioAvailable?.Invoke(this, [.. e.Buffer.Take(e.BytesRecorded)]);

            // Optionally also buffer if you need to "pull" later
            _bufferedWaveProvider?.AddSamples(e.Buffer, 0, e.BytesRecorded);
        };

        _waveIn.StartRecording();
    }

    public void Stop()
    {
        if (_waveIn == null)
            return;

        _waveIn.StopRecording();
        _waveIn.Dispose();
        _waveIn = null;
        _bufferedWaveProvider = null;
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        if (_bufferedWaveProvider == null) return 0;
        return _bufferedWaveProvider.Read(buffer, offset, count);
    }

    /// <summary>
    /// Attempts to read a specified number of bytes from the <see cref="BufferedWaveProvider"/> 
    /// only if enough audio data is currently available in the buffer.
    /// </summary>
    /// <param name="buffer">The target buffer that will receive the audio data if available.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data.</param>
    /// <param name="count">The number of bytes to attempt to read.</param>
    /// <param name="bytesRead">When this method returns, contains the actual number of bytes read into <paramref name="buffer"/>. 
    /// Will be <c>0</c> if insufficient data was available.</param>
    /// <returns><c>true</c> if the requested number of bytes was read successfully; otherwise <c>false</c>.</returns>
    public bool TryRead(byte[] buffer, int offset, int count, out int bytesRead)
    {
        bytesRead = 0;

        if (_bufferedWaveProvider == null)
            return false;

        if (_bufferedWaveProvider.BufferedBytes >= count)
        {
            bytesRead = _bufferedWaveProvider.Read(buffer, offset, count);

            if (bytesRead % 2 == 0)
                ApplyMicGain(buffer, offset, bytesRead);

            return true;
        }

        // not enough data available
        return false;
    }

    private void ApplyMicGain(byte[] pcm, int offset, int numBytes)
    {
        if (numBytes % 2 != 0)
            throw new ArgumentException("PCM range must have even length.");
        
        if (numBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(numBytes), "must be non-negative.");

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "must be non-negative.");

        if (offset + numBytes > pcm.Length)
            throw new ArgumentException("Requested range exceeds buffer length.");

        // Slice the working window
        Span<byte> pcmSlice = pcm.AsSpan(offset, numBytes);

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
    }



    public void Dispose()
    {
        Stop();
    }
}