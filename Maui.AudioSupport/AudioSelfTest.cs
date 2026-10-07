using DigitalVoice.AudioSupport;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Maui.AudioSupport;

/// <summary>
/// Audio-Selbsttest für das Gerät: nimmt einige Sekunden vom Mikrofon auf (über <see cref="AndroidMicrophoneReader"/>
/// und seine Queue), wertet Takt und Pegel aus und spielt die Aufnahme danach über <see cref="AndroidAudioPlayer"/> ab.
/// Die RECORD_AUDIO-Berechtigung muss vorher erteilt sein.
/// </summary>
public static class AudioSelfTest
{
    // Synchron, weil Span-Lokale in async-Methoden erst ab C# 13 erlaubt sind (dieses Projekt: C# 12)
    private static int ComputePeak(List<byte[]> blocks)
    {
        int peak = 0;
        foreach (byte[] block in blocks)
        {
            ReadOnlySpan<short> samples = MemoryMarshal.Cast<byte, short>(block.AsSpan());
            foreach (short s in samples)
                peak = Math.Max(peak, Math.Abs((int)s));
        }
        return peak;
    }

    public static async Task<string> RunAsync(int seconds = 3)
    {
        var sb = new StringBuilder();
        var blocks = new List<byte[]>();
        var stamps = new List<long>();
        var gate = new object();
        long produced, dropped;
        long t0;

        // ---- Aufnahme ----
        using (var mic = new AndroidMicrophoneReader())
        {
            System.Collections.Concurrent.ConcurrentQueue<byte[]> queue = mic.GetPcmQueue();
            mic.AudioAvailable += (_, _) =>
            {
                lock (gate) stamps.Add(Stopwatch.GetTimestamp());
            };

            t0 = Stopwatch.GetTimestamp();
            mic.Start();

            while (Stopwatch.GetElapsedTime(t0).TotalSeconds < seconds)
            {
                while (queue.TryDequeue(out byte[]? block))
                    blocks.Add(block);
                await Task.Delay(5);
            }

            mic.Stop();
            while (queue.TryDequeue(out byte[]? last))
                blocks.Add(last);

            produced = mic.ProducedBlocks;
            dropped = mic.DroppedBlocks;
        }

        // ---- Auswertung ----
        sb.AppendLine($"Recording {seconds} s: {blocks.Count} blocks (expected about {seconds * 50}), dropped {dropped}");

        List<long> ts;
        lock (gate) ts = new List<long>(stamps);

        if (ts.Count > 0)
            sb.AppendLine($"First block after {Stopwatch.GetElapsedTime(t0, ts[0]).TotalMilliseconds:F0} ms");

        if (ts.Count > 1)
        {
            double sum = 0, min = double.MaxValue, max = 0;
            for (int i = 1; i < ts.Count; i++)
            {
                double ms = Stopwatch.GetElapsedTime(ts[i - 1], ts[i]).TotalMilliseconds;
                sum += ms;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
            }
            sb.AppendLine($"Block interval: mean {sum / (ts.Count - 1):F1} ms, min {min:F1}, max {max:F1} (target 20 ms)");
        }

        int peak = ComputePeak(blocks);
        sb.AppendLine($"Level: peak {peak} of 32767{(peak == 0 ? " (MICROPHONE MUTED?)" : "")}");

        // ---- Wiedergabe in Echtzeit ----
        if (blocks.Count > 0)
        {
            using var player = new AndroidAudioPlayer();
            long start = Stopwatch.GetTimestamp();
            for (int i = 0; i < blocks.Count; i++)
            {
                player.FeedPcmData(blocks[i]);
                double wait = (i + 1) * 20.0 - Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                if (wait > 0)
                    await Task.Delay((int)wait);
            }
            await Task.Delay(400); // Rest ausspielen lassen
            sb.AppendLine("Playback: done");
        }

        sb.AppendLine(blocks.Count >= seconds * 45 && dropped == 0 && peak > 0 ? "RESULT: OK" : "RESULT: CHECK");
        return sb.ToString();
    }
}
