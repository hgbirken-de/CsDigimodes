using NLog;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace DigitalVoice.AudioSupport;

/// <summary>
/// Plattformneutrale Erzeuger-Seite für Mikrofon-Reader: nimmt PCM-Bytes beliebiger Stückelung entgegen
/// und reiht daraus fertige Blöcke fester Größe in eine begrenzte <see cref="ConcurrentQueue{T}"/> ein
/// (Standard: 20 ms = 160 Samples = 320 Bytes, 16 bit mono, little endian, Gain bereits angewendet).
/// <para>
/// Ist die Queue voll, wird der älteste Block verworfen (kurzer Aussetzer statt wachsender Verzögerung).
/// Nach jedem eingereihten Block wird <see cref="BlockQueued"/> ausgelöst (Wecker für den Verbraucher,
/// Handler müssen trivial sein).
/// </para>
/// <para>
/// <see cref="Write"/> darf nur von EINEM Thread aufgerufen werden (dem Aufnahme-Thread der Plattform).
/// </para>
/// </summary>
public sealed class PcmBlockQueueWriter
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly int _blockBytes;
    private readonly int _maxQueuedBlocks;

    // Nur vom Aufnahme-Thread angefasst (siehe Write)
    private byte[] _current;
    private int _currentLength;

    private volatile float _gain = 1.0f;
    private float _gainDb;

    private long _producedBlocks;
    private long _droppedBlocks;

    public PcmBlockQueueWriter(int blockBytes = 320, int maxQueuedBlocks = 25)
    {
        if (blockBytes <= 0 || blockBytes % 2 != 0)
            throw new ArgumentOutOfRangeException(nameof(blockBytes), "must be a positive even number (16 bit samples).");
        if (maxQueuedBlocks <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxQueuedBlocks));

        _blockBytes = blockBytes;
        _maxQueuedBlocks = maxQueuedBlocks;
        _current = new byte[blockBytes];
    }

    /// <summary>Die Queue mit den fertigen Blöcken (Verbraucher entnimmt hier).</summary>
    public ConcurrentQueue<byte[]> Queue { get; } = new();

    /// <summary>Wird nach dem Einreihen jedes Blocks ausgelöst (Nutzlast = der Block, nur lesen).</summary>
    public event EventHandler<byte[]>? BlockQueued;

    public int BlockBytes => _blockBytes;

    public long ProducedBlocks => Interlocked.Read(ref _producedBlocks);

    /// <summary>Verworfene Blöcke, weil die Queue voll war (sollte 0 bleiben).</summary>
    public long DroppedBlocks => Interlocked.Read(ref _droppedBlocks);

    /// <summary>Mikrofon-Gain in dB (0 = unverändert). Darf auch während der Aufnahme gesetzt werden.</summary>
    public float GainDb
    {
        get => _gainDb;
        set { _gainDb = value; _gain = (float)Math.Pow(10.0, value / 20.0); }
    }

    /// <summary>Verwirft einen angefangenen Block. Nur aufrufen, solange kein Aufnahme-Thread läuft.</summary>
    public void Reset()
    {
        _currentLength = 0;
    }

    /// <summary>Nimmt PCM-Bytes entgegen und reiht fertige Blöcke ein. Nur vom Aufnahme-Thread aufrufen.</summary>
    public void Write(byte[] buffer, int offset, int count)
    {
        while (count > 0)
        {
            int take = Math.Min(_blockBytes - _currentLength, count);
            Buffer.BlockCopy(buffer, offset, _current, _currentLength, take);
            _currentLength += take;
            offset += take;
            count -= take;

            if (_currentLength == _blockBytes)
            {
                byte[] block = _current;          // Besitz geht an die Queue über
                _current = new byte[_blockBytes];
                _currentLength = 0;

                ApplyGain(block, _gain);
                Enqueue(block);
            }
        }
    }

    private void Enqueue(byte[] block)
    {
        Interlocked.Increment(ref _producedBlocks);

        // ConcurrentQueue ist unbegrenzt -> hier selbst begrenzen: ältesten Block verwerfen
        while (Queue.Count >= _maxQueuedBlocks && Queue.TryDequeue(out _))
        {
            long dropped = Interlocked.Increment(ref _droppedBlocks);
            if (dropped == 1 || dropped % 50 == 0)
                logger.Warn($"PCM queue full: {dropped} block(s) dropped so far (consumer too slow?)");
        }
        Queue.Enqueue(block);

        try
        {
            BlockQueued?.Invoke(this, block);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception in BlockQueued handler.");
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
}
