using NAudio.Wave;
using NLog;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace DigitalVoice.AudioSupport;

/// <summary>
/// Mikrofon-Reader auf NAudio-Basis, der fertige PCM-Blöcke fester Größe in eine vom Aufrufer übergebene
/// <see cref="ConcurrentQueue{T}"/> stellt (Erzeuger-Seite einer Erzeuger/Verbraucher-Kopplung).
/// <para>
/// Jeder Block hat exakt <see cref="BlockBytes"/> Bytes (Standard: 20 ms = 160 Samples = 320 Bytes, 16 bit,
/// little endian) und ist bereits mit <see cref="GainDb"/> bearbeitet.
/// </para>
/// <para>
/// Der NAudio-Aufnahme-Thread blockiert nie. Die Queue wird auf <c>maxQueuedBlocks</c> begrenzt, ist sie voll,
/// wird der älteste Block verworfen (kurzer Aussetzer statt wachsender Verzögerung).
/// </para>
/// <para>
/// <see cref="AudioAvailable"/> dient als Wecker für den Verbraucher: Das Event wird NACH dem Einreihen
/// ausgelöst (Nutzlast = der gerade eingereihte Block, nur lesen!). Handler müssen trivial sein
/// (z.B. <c>AutoResetEvent.Set()</c>), sie laufen auf dem Aufnahme-Thread.
/// </para>
/// <para>
/// Das Pull-Verfahren (<see cref="Read"/>, <see cref="TryRead"/>) wird von dieser Klasse nicht unterstützt.
/// </para>
/// </summary>
public sealed class MicrophoneReader2(
    int sampleRate = 8000,
    int channels = 1,
    int maxQueuedBlocks = 25,
    int blockMilliseconds = 20) : IMicrophoneReader, IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public WaveFormat Format { get; } = new WaveFormat(sampleRate, 16, channels);

    public event EventHandler<byte[]>? AudioAvailable;

    public ConcurrentQueue<byte[]> _pcmQueue = new();

    private readonly int _maxQueuedBlocks = maxQueuedBlocks > 0 ? maxQueuedBlocks : throw new ArgumentOutOfRangeException(nameof(maxQueuedBlocks));
    private readonly int _blockMilliseconds = blockMilliseconds;
    private readonly int _blockBytes = CalcBlockBytes(sampleRate, channels, blockMilliseconds);

    private readonly object _lock = new();
    private WaveInEvent? _waveIn;
    private volatile bool _running;

    // Block, der gerade gefüllt wird. Wird nur vom NAudio-Aufnahme-Thread angefasst
    // (DataAvailable wird nacheinander auf EINEM Thread aufgerufen) -> keine Sperre nötig.
    private byte[] _current = [];
    private int _currentLength;

    private volatile float _gain = 1.0f;

    // Gain in dB (default 0 = unity)
    private float _gainDb = 0.0f;

    private long _producedBlocks;
    private long _droppedBlocks;

    /// <summary>Größe eines Blocks in Bytes (bei 8 kHz/16 bit/mono/20 ms: 320).</summary>
    public int BlockBytes => _blockBytes;

    /// <summary>Anzahl der bisher erzeugten Blöcke (zur Diagnose).</summary>
    public long ProducedBlocks => Interlocked.Read(ref _producedBlocks);

    /// <summary>Anzahl der verworfenen Blöcke, weil die Queue voll war (zur Diagnose; sollte 0 bleiben).</summary>
    public long DroppedBlocks => Interlocked.Read(ref _droppedBlocks);

    /// <summary>Mikrofon-Gain in dB (0 = unverändert). Darf auch während der Aufnahme gesetzt werden.</summary>
    public float GainDb
    {
        get => _gainDb;
        set { _gainDb = value; _gain = (float)Math.Pow(10.0, value / 20.0); }
    }

    public ConcurrentQueue<byte[]> GetPcmQueue()
    {
        return _pcmQueue;
    }

    /// <summary>
    /// Start this MicrophoneReader instance.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            if (_waveIn != null)
                return; // already running

            _current = new byte[_blockBytes];
            _currentLength = 0;

            var waveIn = new WaveInEvent
            {
                DeviceNumber = 0,                       // default input
                WaveFormat = Format,
                BufferMilliseconds = _blockMilliseconds, // ein Block pro Event (NAudio-Standard wären 100 ms)
                NumberOfBuffers = 6,                     // Reserve, falls der Aufnahme-Thread mal verzögert wird
            };
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;

            try
            {
                _running = true;
                waveIn.StartRecording();
                _waveIn = waveIn;
            }
            catch
            {
                _running = false;
                waveIn.DataAvailable -= OnDataAvailable;
                waveIn.RecordingStopped -= OnRecordingStopped;
                waveIn.Dispose();
                throw;
            }
        }
    }

    /// <summary>
    /// Stoppt die Aufnahme. Danach werden keine neuen Blöcke mehr eingereiht (höchstens ein gerade
    /// laufender). Die Queue wird NICHT geleert, das ist Sache des Verbrauchers.
    /// </summary>
    public void Stop()
    {
        WaveInEvent? waveIn;
        lock (_lock)
        {
            _running = false;
            waveIn = _waveIn;
            _waveIn = null;
        }

        if (waveIn == null)
            return;

        waveIn.DataAvailable -= OnDataAvailable;
        waveIn.RecordingStopped -= OnRecordingStopped;
        try { waveIn.StopRecording(); }
        finally { waveIn.Dispose(); }
    }

    // Aus IMicrophoneReader: Pull-Verfahren wird hier bewusst nicht unterstützt (Daten kommen über die Queue).
    public int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException($"{nameof(MicrophoneReader2)} liefert PCM-Blöcke über die Queue, nicht per Read().");

    public bool TryRead(byte[] buffer, int offset, int count, out int bytesRead) =>
        throw new NotSupportedException($"{nameof(MicrophoneReader2)} liefert PCM-Blöcke über die Queue, nicht per TryRead().");

    public void Dispose()
    {
        Stop();
    }

    // Läuft auf dem NAudio-Aufnahme-Thread: sammeln, Gain anwenden, einreihen - nie blockieren.
    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_running)
            return;

        int offset = 0;
        int remaining = e.BytesRecorded;
        while (remaining > 0)
        {
            // Der Treiber muss nicht exakt Blockgröße liefern -> in feste Blöcke umgruppieren
            int take = Math.Min(_blockBytes - _currentLength, remaining);
            Buffer.BlockCopy(e.Buffer, offset, _current, _currentLength, take);
            _currentLength += take;
            offset += take;
            remaining -= take;

            if (_currentLength == _blockBytes)
            {
                byte[] block = _current;            // Besitz geht an die Queue über
                _current = new byte[_blockBytes];
                _currentLength = 0;

                ApplyMicGain(block, 0, block.Length);
                Enqueue(block);
            }
        }
    }

    private void Enqueue(byte[] block)
    {
        Interlocked.Increment(ref _producedBlocks);

        // ConcurrentQueue ist unbegrenzt -> hier selbst begrenzen: ältesten Block verwerfen
        while (_pcmQueue.Count >= _maxQueuedBlocks && _pcmQueue.TryDequeue(out _))
        {
            long dropped = Interlocked.Increment(ref _droppedBlocks);
            if (dropped == 1 || dropped % 50 == 0)
                logger.Warn($"PCM queue full: {dropped} block(s) dropped so far (consumer too slow?)");
        }
        _pcmQueue.Enqueue(block);

        try
        {
            AudioAvailable?.Invoke(this, block); // Wecker für den Verbraucher
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception in AudioAvailable handler.");
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
            logger.Error(e.Exception, "Microphone recording stopped unexpectedly.");
    }

    private static int CalcBlockBytes(int sampleRate, int channels, int blockMilliseconds)
    {
        var format = new WaveFormat(sampleRate, 16, channels);
        int bytes = format.AverageBytesPerSecond * blockMilliseconds / 1000;
        if (bytes <= 0 || bytes % format.BlockAlign != 0)
            throw new ArgumentException($"blockMilliseconds={blockMilliseconds} ergibt keinen ganzzahligen PCM-Block bei {sampleRate} Hz.", nameof(blockMilliseconds));
        return bytes;
    }

    private void ApplyMicGain(byte[] pcm, int offset, int numBytes)
    {
        float gain = _gain;
        if (Math.Abs(gain - 1.0f) < 0.0001f)
            return;

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
            int scaled = (int)(samples[i] * gain);

            // Saturation clamp
            if (scaled > short.MaxValue) scaled = short.MaxValue;
            else if (scaled < short.MinValue) scaled = short.MinValue;

            samples[i] = (short)scaled;
        }
    }
}
