using Android.Media;
using DigitalVoice.AudioSupport;
using NLog;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Maui.AudioSupport;

/// <summary>
/// Audio-Wiedergabe für Android auf Basis von <see cref="AudioTrack"/> (Gegenstück zum NAudio-basierten
/// <c>AudioPlayer</c> unter Windows). Erwartet PCM 16 bit mono little endian (Standard: 8 kHz).
/// <para>
/// <see cref="FeedPcmData"/> blockiert nie: Die Daten werden kopiert (der Aufrufer-Puffer bleibt unverändert, auch
/// der Gain wirkt nur auf die Kopie), in eine begrenzte Queue gestellt und von einem eigenen Thread in den
/// AudioTrack geschrieben. Ist die Queue voll (Standard 1 s), wird das älteste Stück verworfen.
/// </para>
/// </summary>
public sealed class AndroidAudioPlayer : IAudioPlayer, IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly AudioTrack _track;
    private readonly ConcurrentQueue<byte[]> _queue = new();
    private readonly AutoResetEvent _signal = new(false);
    private readonly Thread _thread;
    private readonly int _maxQueuedBytes;

    private int _queuedBytes;
    private long _droppedChunks;
    private volatile bool _disposed;

    private volatile float _gain = 1.0f;
    private float _gainDb;

    /// <summary>Wiedergabe-Gain in dB (0 = unverändert).</summary>
    public float GainDb
    {
        get => _gainDb;
        set { _gainDb = value; _gain = (float)Math.Pow(10.0, value / 20.0); }
    }

    /// <param name="sampleRate">Abtastrate in Hz (Standard 8000).</param>
    /// <param name="maxBufferedMs">Obergrenze für das, was vor dem AudioTrack wartet (ms).</param>
    public AndroidAudioPlayer(int sampleRate = 8000, int maxBufferedMs = 1000)
    {
        int minBuffer = AudioTrack.GetMinBufferSize(sampleRate, ChannelOut.Mono, Encoding.Pcm16bit);
        if (minBuffer <= 0)
            throw new InvalidOperationException($"AudioTrack does not support {sampleRate} Hz mono 16 bit (minBufferSize={minBuffer}).");

        // mindestens 200 ms Puffer im AudioTrack: fängt kleine Verzögerungen der Zuführung ab
        int bufferSize = Math.Max(minBuffer * 2, sampleRate * 2 / 5);

        var attributes = new AudioAttributes.Builder()!
            .SetUsage(AudioUsageKind.Media)!             // Lautsprecher (VoiceCommunication würde ggf. auf den Hörer routen)
            .SetContentType(AudioContentType.Speech)!
            .Build()!;

        var format = new AudioFormat.Builder()!
            .SetEncoding(Encoding.Pcm16bit)!
            .SetSampleRate(sampleRate)!
            .SetChannelMask(ChannelOut.Mono)!
            .Build()!;

        _track = new AudioTrack(attributes, format, bufferSize, AudioTrackMode.Stream, AudioManager.AudioSessionIdGenerate);
        if (_track.State != AudioTrackState.Initialized)
        {
            _track.Release();
            throw new InvalidOperationException("AudioTrack could not be initialized.");
        }

        _maxQueuedBytes = sampleRate * 2 * maxBufferedMs / 1000;

        _track.Play();
        _thread = new Thread(PlaybackLoop) { IsBackground = true, Name = "AndroidAudioPlayer" };
        _thread.Start();

        logger.Debug($"Started: {sampleRate} Hz, trackBuffer={bufferSize} bytes, maxQueued={_maxQueuedBytes} bytes");
    }

    /// <summary>Stellt PCM-Daten zur Wiedergabe ein. Blockiert nie.</summary>
    public void FeedPcmData(byte[] pcmData, int offset = 0, int count = -1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (pcmData == null || pcmData.Length == 0)
            return;

        if (count == -1)
            count = pcmData.Length - offset;

        count &= ~1; // nur ganze 16-bit-Samples
        if (count <= 0)
            return;

        byte[] chunk = new byte[count];
        Buffer.BlockCopy(pcmData, offset, chunk, 0, count);
        ApplyGain(chunk, _gain);

        // begrenzen: ältestes Stück verwerfen
        while (Volatile.Read(ref _queuedBytes) + count > _maxQueuedBytes && _queue.TryDequeue(out byte[]? old))
        {
            Interlocked.Add(ref _queuedBytes, -old.Length);
            long dropped = Interlocked.Increment(ref _droppedChunks);
            if (dropped == 1 || dropped % 50 == 0)
                logger.Warn($"Playback queue full: {dropped} chunk(s) dropped so far.");
        }

        _queue.Enqueue(chunk);
        Interlocked.Add(ref _queuedBytes, count);
        _signal.Set();
    }

    // Eigener Thread: AudioTrack.Write blockiert, bis im AudioTrack-Puffer Platz ist
    private void PlaybackLoop()
    {
        // Eine unbehandelte Exception auf einem eigenen Thread würde die ganze App beenden -> loggen statt abstürzen
        try
        {
            Android.OS.Process.SetThreadPriority(Android.OS.ThreadPriority.UrgentAudio);

            while (!_disposed)
            {
                _signal.WaitOne(100);

                while (!_disposed && _queue.TryDequeue(out byte[]? chunk))
                {
                    Interlocked.Add(ref _queuedBytes, -chunk.Length);
                    int written = _track.Write(chunk, 0, chunk.Length);
                    if (written < 0)
                        logger.Error($"AudioTrack.Write failed (result={written}).");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception in playback loop.");
        }
    }

    private static void ApplyGain(byte[] pcm, float gain)
    {
        if (Math.Abs(gain - 1.0f) < 0.0001f)
            return;

        Span<short> samples = MemoryMarshal.Cast<byte, short>(pcm.AsSpan());
        for (int i = 0; i < samples.Length; i++)
        {
            int scaled = (int)(samples[i] * gain);
            if (scaled > short.MaxValue) scaled = short.MaxValue;
            else if (scaled < short.MinValue) scaled = short.MinValue;
            samples[i] = (short)scaled;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _signal.Set();
        _thread.Join(500);

        try { _track.Stop(); }
        catch (Java.Lang.IllegalStateException) { /* war nicht gestartet */ }
        _track.Release();
        _signal.Dispose();
    }
}
