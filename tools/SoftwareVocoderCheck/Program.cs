using DigitalVoice.AmbeSupport;
using DigitalVoice.SoftwareVocoder;
using System.Diagnostics;

// Prüft den C#-Decoder und den AmbeSoftwareController gegen Referenzdaten aus dem C++-Original (DroidStar/mbelib).
// golden/<name>.bin: Frames als [Modus:1][Länge:1][Daten]  (Modus 0 = DMR, 1 = YSF/NXDN, 2 = D-STAR)
// golden/<name>.pcm: erwartete PCM-Samples (16 Bit, little endian), 160 je Frame
// Aufruf: SoftwareVocoderCheck            (Prüfung)
//         SoftwareVocoderCheck --bench    (zusätzlich Laufzeitmessung)

const int Tolerance = 2;   // LSB

string dir = Path.Combine(AppContext.BaseDirectory, "golden");
bool failed = false;

short[] LoadPcm(string file)
{
    byte[] bytes = File.ReadAllBytes(file);
    var samples = new short[bytes.Length / 2];
    Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);
    return samples;
}

List<byte[]> LoadFrames(string file)
{
    byte[] input = File.ReadAllBytes(file);
    var frames = new List<byte[]>();
    for (int pos = 0; pos + 2 <= input.Length;)
    {
        int len = input[pos + 1];
        frames.Add(input.AsSpan(pos + 2, len).ToArray());
        pos += 2 + len;
    }
    return frames;
}


// Die Clients ordnen die 49 Bits (YSF, FCS, NXDN) für den DVSI-Chip um. Hier die beiden Verfahren der Clients, nachgebaut:
//  - NXDN:   NxdnCodec.Interleave        (dvsiData[Tabelle[i]] = Luftbit i)
//  - Fusion: AmbeExtractor (YSF/FCS)      (Chipbit i = Luftbit AMBE_OUTPUT_BIT_INTERLEAVE_TABLE[i])
// Beide ergeben dieselbe Reihenfolge. Die Testframes in golden/ liegen in Luft-Reihenfolge vor.
int[] nxdnTable =
[
    0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 41, 43, 45, 47,
    1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34, 37, 40, 42, 44, 46, 48,
    2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35, 38,
];
int[] fusionTable =
[
    0, 18, 36, 1, 19, 37, 2, 20, 38, 3, 21, 39, 4, 22, 40, 5, 23, 41, 6, 24,
    42, 7, 25, 43, 8, 26, 44, 9, 27, 45, 10, 28, 46, 11, 29, 47, 12, 30, 48, 13,
    31, 14, 32, 15, 33, 16, 34, 17, 35,
];

static int Bit(byte[] data, int index) => (data[index >> 3] >> (7 - (index & 7))) & 1;

byte[] NxdnInterleave(byte[] air)
{
    var dvsi = new byte[7];
    for (int i = 0; i < 49; i++)
    {
        int j = nxdnTable[i];
        dvsi[j / 8] += (byte)(Bit(air, i) << (7 - (j % 8)));
    }
    return dvsi;
}

byte[] FusionInterleave(byte[] air)
{
    var dvsi = new byte[7];
    for (int i = 0; i < 49; i++)
        dvsi[i / 8] |= (byte)(Bit(air, fusionTable[i]) << (7 - (i % 8)));
    return dvsi;
}

// ---------------------------------------------------------------------------------------------------
// 1) Decoder
// ---------------------------------------------------------------------------------------------------
Console.WriteLine("Decoder (AmbeSoftwareDecoder):");
foreach (string binFile in Directory.GetFiles(dir, "*.bin").OrderBy(f => f))
{
    string name = Path.GetFileNameWithoutExtension(binFile);
    byte[] input = File.ReadAllBytes(binFile);
    short[] expected = LoadPcm(Path.ChangeExtension(binFile, ".pcm"));

    var decoder = new AmbeSoftwareDecoder(new NoiseSource(12345u)); // dieselbe Zufallsfolge wie die Referenz
    var pcm = new short[160];
    int frame = 0, maxDiff = 0, deviating = 0;

    for (int pos = 0; pos + 2 <= input.Length; frame++)
    {
        int mode = input[pos++], len = input[pos++];
        var data = input.AsSpan(pos, len);
        pos += len;

        switch (mode)
        {
            case 0: decoder.Decode2450x1150(data, pcm); break;
            case 1: decoder.Decode2450(data, pcm); break;
            default: decoder.Decode2400x1200(data, pcm); break;
        }

        int frameMax = 0;
        for (int i = 0; i < 160; i++)
            frameMax = Math.Max(frameMax, Math.Abs(pcm[i] - expected[frame * 160 + i]));

        maxDiff = Math.Max(maxDiff, frameMax);
        if (frameMax > 0) deviating++;
    }

    bool ok = maxDiff <= Tolerance && frame * 160 == expected.Length;
    failed |= !ok;
    Console.WriteLine($"  {(ok ? "OK    " : "FEHLER")} {name,-14} {frame,4} Frames, größte Abweichung {maxDiff} LSB, Frames mit Abweichung: {deviating}");
}

// ---------------------------------------------------------------------------------------------------
// 2) AmbeSoftwareController: so, wie die Clients ihn benutzen
//    (Channel-Paket-Array wird wiederverwendet; n Pakete senden, danach n Antworten lesen; PCM Big Endian -> Little Endian)
// ---------------------------------------------------------------------------------------------------
Console.WriteLine("Controller (AmbeSoftwareController):");

(bool ok, int frames, int maxDiff) RunController(string name, byte[] ratePacket, byte[] template, int dataLen, int perBurst, Func<byte[], byte[]>? toChipOrder = null)
{
    var ctrl = new AmbeSoftwareController(new NoiseSource(12345u));
    ctrl.Open();
    ctrl.SendReceivePacket(ratePacket);

    List<byte[]> frames = LoadFrames(Path.Combine(dir, name + ".bin"));
    short[] expected = LoadPcm(Path.Combine(dir, name + ".pcm"));
    int maxDiff = 0, done = 0;

    for (int start = 0; start < frames.Count; start += perBurst)
    {
        int n = Math.Min(perBurst, frames.Count - start);
        for (int i = 0; i < n; i++)
        {
            Buffer.BlockCopy(toChipOrder != null ? toChipOrder(frames[start + i]) : frames[start + i], 0, template, 6, dataLen);
            ctrl.SendPacket(template);
        }
        for (int i = 0; i < n; i++)
        {
            byte[]? response = ctrl.ReceivePacket();
            if (!AmbeHelper.IsSpeechPacket(response) || response!.Length != 326)
                return (false, done, int.MaxValue);

            AmbeHelper.SwapPcmBytes(response);
            for (int s = 0; s < 160; s++)
            {
                short value = (short)(response[6 + (2 * s)] | (response[7 + (2 * s)] << 8));
                maxDiff = Math.Max(maxDiff, Math.Abs(value - expected[((start + i) * 160) + s]));
            }
            done++;
        }
    }

    return (maxDiff <= Tolerance && done * 160 == expected.Length, done, maxDiff);
}

var runs = new (string label, string file, byte[] rate, byte[] template, int len, int burst, Func<byte[], byte[]>? convert)[]
{
    ("DMR (je 3 Frames)", "dmr",
        [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x04, 0x31, 0x07, 0x54, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6f, 0x48],
        [0x61, 0x00, 0x0b, 0x01, 0x01, 0x48, 0, 0, 0, 0, 0, 0, 0, 0, 0], 9, 3, null),
    ("NXDN, Chip-Reihenfolge (je 5)", "ysf_nxdn",
        [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x04, 0x31, 0x07, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x31],
        [0x61, 0x00, 0x09, 0x01, 0x01, 0x31, 0, 0, 0, 0, 0, 0, 0], 7, 5, NxdnInterleave),
    ("YSF/FCS, Chip-Reihenfolge (je 5)", "ysf_nxdn",
        [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x04, 0x31, 0x07, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x31],
        [0x61, 0x00, 0x09, 0x01, 0x01, 0x31, 0, 0, 0, 0, 0, 0, 0], 7, 5, FusionInterleave),
    ("D-STAR (je 3 Frames)", "dstar",
        [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x01, 0x30, 0x07, 0x63, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48],
        [0x61, 0x00, 0x0b, 0x01, 0x01, 0x48, 0, 0, 0, 0, 0, 0, 0, 0, 0], 9, 3, null),
};

foreach (var r in runs)
{
    var (ok, frames, maxDiff) = RunController(r.file, r.rate, r.template, r.len, r.burst, r.convert);
    failed |= !ok;
    Console.WriteLine($"  {(ok ? "OK    " : "FEHLER")} {r.label,-34} {frames,4} Frames, größte Abweichung {(maxDiff == int.MaxValue ? "ungültige Antwort" : maxDiff + " LSB")}");
}


{
    // Beide Client-Verfahren müssen für jeden denkbaren Frame dieselben Bytes liefern
    var random = new Random(1);
    bool same = true;
    for (int n = 0; n < 2000 && same; n++)
    {
        var air = new byte[7];
        random.NextBytes(air);
        air[6] &= 0x80;   // nur 49 Bit
        same = NxdnInterleave(air).SequenceEqual(FusionInterleave(air));
    }
    failed |= !same;
    Console.WriteLine($"  {(same ? "OK    " : "FEHLER")} NxdnCodec.Interleave und Fusion-AmbeExtractor ordnen die 49 Bits gleich um");
}

// Sonderfälle: Kennung, Reset, Kodieren, Freigabe
{
    var ctrl = new AmbeSoftwareController();
    byte[]? ready = ctrl.SendReceivePacket([0x61, 0x00, 0x01, 0x00, 0x33]);
    byte[]? id = ctrl.SendReceivePacket([0x61, 0x00, 0x01, 0x00, 0x30]);
    var speech = new byte[326];
    new byte[] { 0x61, 0x01, 0x42, 0x02, 0x00, 0xA0 }.CopyTo(speech, 0);
    bool noAnswer = ctrl.SendReceivePacket(speech) == null && !ctrl.CanEncode;
    ctrl.Dispose();
    bool disposedOk = ctrl.ReceivePacket() == null && !ctrl.IsOpen;

    bool ok = ready is { Length: 5 } && ready[4] == 0x39 && id is { Length: > 6 } && id[4] == 0x30 && noAnswer && disposedOk;
    failed |= !ok;
    Console.WriteLine($"  {(ok ? "OK    " : "FEHLER")} Reset/READY, Product-ID, Kodieren nicht möglich, Verhalten nach Dispose");
}

if (args.Contains("--bench"))
{
    List<byte[]> frames = LoadFrames(Path.Combine(dir, "dmr.bin"));
    var decoder = new AmbeSoftwareDecoder(new NoiseSource(1u));
    var pcm = new short[160];

    for (int w = 0; w < 5; w++) foreach (var f in frames) decoder.Decode2450x1150(f, pcm);   // Aufwärmen
    var sw = Stopwatch.StartNew();
    int count = 0;
    for (int r = 0; r < 40; r++) foreach (var f in frames) { decoder.Decode2450x1150(f, pcm); count++; }
    sw.Stop();
    Console.WriteLine($"Laufzeit: {sw.Elapsed.TotalMilliseconds / count:F3} ms je Frame (Budget 20 ms)");
}

Console.WriteLine(failed ? "FEHLER" : "ALLE PRÜFUNGEN BESTANDEN");
return failed ? 1 : 0;
