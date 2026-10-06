using Android.Media;
using DigitalVoice.AudioSupport;
using NLog;
using System.Collections.Concurrent;

namespace Maui.AudioSupport;

/// <summary>
/// Mikrofon-Reader für Android auf Basis von <see cref="AudioRecord"/> (Gegenstück zum NAudio-basierten
/// <c>MicrophoneReader</c> unter Windows). Gleicher Vertrag: fertige PCM-Blöcke fester Größe (Standard 320 Bytes =
/// 20 ms bei 8 kHz, 16 bit mono) landen in einer begrenzten Queue (<see cref="GetPcmQueue"/>), <see cref="AudioAvailable"/>
/// ist der Wecker für den Verbraucher. Das Pull-Verfahren (<see cref="Read"/>/<see cref="TryRead"/>) wird nicht unterstützt.
/// <para>
/// Voraussetzung: Die Laufzeit-Berechtigung RECORD_AUDIO muss VOR <see cref="Start"/> erteilt sein (in der App per
/// <c>Permissions.RequestAsync&lt;Permissions.Microphone&gt;()</c>), sonst scheitert die Initialisierung.
/// </para>
/// </summary>
public sealed class AndroidMicrophoneReader : IMicrophoneReader
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly int _sampleRate;
    private readonly PcmBlockQueueWriter _writer;
    private readonly object _lock = new();

    private AudioRecord? _record;
    private Thread? _thread;
    private volatile bool _running;

    public AndroidMicrophoneReader(int sampleRate = 8000, int maxQueuedBlocks = 25, int blockMilliseconds = 20)
    {
        _sampleRate = sampleRate;
        int blockBytes = sampleRate * 2 * blockMilliseconds / 1000; // 16 bit mono
        _writer = new PcmBlockQueueWriter(blockBytes, maxQueuedBlocks);
        _writer.BlockQueued += (_, block) => AudioAvailable?.Invoke(this, block);
    }

    public event EventHandler<byte[]>? AudioAvailable;

    public float GainDb
    {
        get => _writer.GainDb;
        set => _writer.GainDb = value;
    }

    public long ProducedBlocks => _writer.ProducedBlocks;

    public long DroppedBlocks => _writer.DroppedBlocks;

    public ConcurrentQueue<byte[]> GetPcmQueue() => _writer.Queue;

    public void Start()
    {
        lock (_lock)
        {
            if (_running)
                return;

            int minBuffer = AudioRecord.GetMinBufferSize(_sampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
            if (minBuffer <= 0)
                throw new InvalidOperationException($"AudioRecord does not support {_sampleRate} Hz mono 16 bit (minBufferSize={minBuffer}).");

            int bufferSize = Math.Max(minBuffer, _writer.BlockBytes * 8);

            var record = new AudioRecord(AudioSource.Mic, _sampleRate, ChannelIn.Mono, Encoding.Pcm16bit, bufferSize);
            if (record.State != State.Initialized)
            {
                record.Release();
                throw new InvalidOperationException(
                    "AudioRecord could not be initialized. RECORD_AUDIO permission missing, microphone in use, or the device does not support the format.");
            }

            _writer.Reset();
            record.StartRecording();

            _record = record;
            _running = true;
            _thread = new Thread(ReadLoop) { IsBackground = true, Name = "AndroidMicrophoneReader" };
            _thread.Start();

            logger.Debug($"Started: {_sampleRate} Hz, block={_writer.BlockBytes} bytes, buffer={bufferSize} bytes");
        }
    }

    public void Stop()
    {
        AudioRecord? record;
        Thread? thread;
        lock (_lock)
        {
            _running = false;
            record = _record;
            thread = _thread;
            _record = null;
            _thread = null;
        }

        if (record == null)
            return;

        try { record.Stop(); }
        catch (Java.Lang.IllegalStateException) { /* war nicht gestartet */ }

        thread?.Join(500);
        record.Release();
        logger.Debug("Stopped.");
    }

    // Läuft auf einem eigenen Thread: AudioRecord.Read blockiert, bis Daten da sind (Takt kommt vom Audio-Treiber)
    private void ReadLoop()
    {
        // Eine unbehandelte Exception auf einem eigenen Thread würde die ganze App beenden -> loggen statt abstürzen
        try
        {
            Android.OS.Process.SetThreadPriority(Android.OS.ThreadPriority.UrgentAudio);

            AudioRecord? record = _record;
            if (record == null)
                return;

            byte[] buffer = new byte[_writer.BlockBytes];
            while (_running)
            {
                int n = record.Read(buffer, 0, buffer.Length);
                if (n > 0)
                {
                    _writer.Write(buffer, 0, n);
                }
                else if (n < 0)
                {
                    logger.Error($"AudioRecord.Read failed (result={n}), stopping the read loop.");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception in microphone read loop.");
        }
    }

    // Aus IMicrophoneReader: Pull-Verfahren wird bewusst nicht unterstützt (Daten kommen über die Queue).
    public int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException($"{nameof(AndroidMicrophoneReader)} liefert PCM-Blöcke über die Queue, nicht per Read().");

    public bool TryRead(byte[] buffer, int offset, int count, out int bytesRead) =>
        throw new NotSupportedException($"{nameof(AndroidMicrophoneReader)} liefert PCM-Blöcke über die Queue, nicht per TryRead().");

    public void Dispose() => Stop();
}
