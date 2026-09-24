
using DigitalVoice.AmbeSupport;
using NAudio.Wave;
using System.Buffers.Binary;
using System.Diagnostics;
using Xunit.Abstractions;

namespace DigitalVoiceTest;

public class UnitTest_AMBE3000RController
{
    readonly ITestOutputHelper output;

    static readonly string portName = "COM13"; // <=== define the serial port here!

    static readonly Ambe3000RController controller;
    static UnitTest_AMBE3000RController()
    {
        controller = new(portName);
        controller.Open();
    }

    public UnitTest_AMBE3000RController(ITestOutputHelper output)
    {
        this.output = output;
        output.WriteLine($"port name: {portName}");
    }

    internal void Dispose()
    {
        output.WriteLine("Dispose()");
        controller.Close();
    }

    [Fact]
    public void Test_Decode()
    {
        byte[] ambe = [0xa6, 0x63, 0x2d, 0x67, 0xa9, 0xfb, 0x00]; // <== test data
        Stopwatch stopwatch = new();
        stopwatch.Start();
        short[] pcm = controller.Decode(ambe, ambe.Length);
        stopwatch.Stop();
        output.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        output.WriteLine($"pcm length: {pcm.Length}");
        Assert.Equal(160, pcm.Length);

        //byte[] ambe_out = controller.Encode(pcm);
        //Assert.Equal(6, ambe_out.Length);
    }

    [Fact]
    public void Test_Encode()
    {
        var packet = new byte[] // source: wireshark
        {
            0x61, 0x01, 0x42, 0x02, 0x00, 0xA0, 0xFF, 0xEB,
            0x00, 0x16, 0xFF, 0xFA, 0x00, 0x2D, 0x00, 0x22,
            0x00, 0x2A, 0x00, 0x1F, 0x00, 0x06, 0x00, 0x1C,
            0x00, 0x06, 0x00, 0x1E, 0xFF, 0xFE, 0x00, 0x06,
            0xFF, 0xEE, 0x00, 0x19, 0x00, 0x10, 0x00, 0x00,
            0x00, 0x0F, 0xFF, 0xD9, 0xFF, 0xF5, 0xFF, 0xD5,
            0xFF, 0xE4, 0xFF, 0xDC, 0xFF, 0xDF, 0xFF, 0xDC,
            0xFF, 0xCA, 0xFF, 0xDA, 0xFF, 0xC6, 0xFF, 0xDB,
            0xFF, 0xC7, 0xFF, 0xDF, 0xFF, 0xE6, 0x00, 0x00,
            0x00, 0x1E, 0x00, 0x17, 0x00, 0x22, 0x00, 0x13,
            0x00, 0x12, 0x00, 0x1F, 0x00, 0x1B, 0x00, 0x19,
            0x00, 0x0D, 0x00, 0x03, 0x00, 0x16, 0x00, 0x06,
            0x00, 0x13, 0xFF, 0xE8, 0xFF, 0xEB, 0xFF, 0xE6,
            0xFF, 0xF5, 0x00, 0x04, 0xFF, 0xF6, 0x00, 0x08,
            0xFF, 0xFE, 0x00, 0x32, 0x00, 0x1D, 0x00, 0x34,
            0xFF, 0xFD, 0x00, 0x18, 0x00, 0x1B, 0x00, 0x3A,
            0x00, 0x5C, 0x00, 0x2F, 0x00, 0x66, 0x00, 0x27,
            0x00, 0x5E, 0x00, 0x14, 0x00, 0x0F, 0xFF, 0xBF,
            0xFF, 0xAA, 0xFF, 0xC5, 0xFF, 0xA2, 0xFF, 0xE3,
            0xFF, 0x99, 0xFF, 0xDF, 0xFF, 0xBB, 0xFF, 0xF8,
            0xFF, 0xDE, 0xFF, 0xCF, 0xFF, 0xE0, 0xFF, 0xC6,
            0x00, 0x16, 0x00, 0x01, 0x00, 0x35, 0x00, 0x12,
            0x00, 0x22, 0x00, 0x16, 0x00, 0x1C, 0x00, 0x13,
            0xFF, 0xFE, 0xFF, 0xEB, 0xFF, 0xE1, 0xFF, 0xF9,
            0xFF, 0xF6, 0x00, 0x14, 0xFF, 0xF6, 0xFF, 0xFE,
            0xFF, 0xE7, 0xFF, 0xEE, 0xFF, 0xE3, 0xFF, 0xDC,
            0xFF, 0xD1, 0xFF, 0xCA, 0xFF, 0xDC, 0xFF, 0xF2,
            0x00, 0x0F, 0x00, 0x1D, 0x00, 0x1F, 0x00, 0x21,
            0x00, 0x2A, 0x00, 0x2A, 0x00, 0x3D, 0x00, 0x16,
            0x00, 0x26, 0x00, 0x08, 0x00, 0x2B, 0x00, 0x2C,
            0x00, 0x2A, 0x00, 0x17, 0x00, 0x0A, 0x00, 0x12,
            0xFF, 0xFE, 0x00, 0x1B, 0xFF, 0xE3, 0x00, 0x0A,
            0xFF, 0xEA, 0x00, 0x29, 0x00, 0x1F, 0x00, 0x41,
            0x00, 0x34, 0x00, 0x1D, 0x00, 0x2D, 0xFF, 0xE4,
            0x00, 0x00, 0xFF, 0x97, 0xFF, 0xCC, 0xFF, 0x8A,
            0xFF, 0xBA, 0xFF, 0xA6, 0xFF, 0x9B, 0xFF, 0xB8,
            0xFF, 0x90, 0xFF, 0xCC, 0xFF, 0x93, 0xFF, 0xD4,
            0xFF, 0xB5, 0xFF, 0xFF, 0x00, 0x07, 0x00, 0x2F,
            0x00, 0x3C, 0x00, 0x3E, 0x00, 0x59, 0x00, 0x3D,
            0x00, 0x52, 0x00, 0x25, 0x00, 0x50, 0x00, 0x50,
            0x00, 0x8C, 0x00, 0x87, 0x00, 0x87
        };

        ReadOnlySpan<byte> payload = packet.AsSpan(6);
        int sampleCount = payload.Length / 2;
        short[] pcm = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            // reads two bytes BE -> Int16 correctly, returns in host endianness
            pcm[i] = BinaryPrimitives.ReadInt16BigEndian(payload.Slice(i * 2, 2));
        }
        Stopwatch stopwatch = new();
        stopwatch.Start();
        byte[] ambe = controller.Encode(pcm);
        stopwatch.Stop();
        output.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        output.WriteLine($"AMBE length: {ambe.Length}");
        //Assert.Equal(6, ambe.Length);

        //byte[] ambe_out = client.Encode(pcm);
        //Assert.Equal(6, ambe_out.Length);
    }

    [Fact]
    public void Test_GetProductId()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        string? productId = controller.GetProductId();
        stopwatch.Stop();
        output.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        output.WriteLine($"product info: {productId}");
    }

    [Fact]
    public void Test_GetVersion()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        string? version = controller.GetVersion();
        stopwatch.Stop();
        output.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        output.WriteLine($"version: {version}");
    }

    [Fact]
    public void Test_SendReceivePacket()
    {
        byte[] packet = [0x61, 0x00, 0x09, 0x01, 0x01, 0x31, 0xb0, 0x3b, 0x1b, 0x4f, 0xce, 0xa7, 0x00];
        Stopwatch stopwatch = new();
        stopwatch.Start();
        byte[]? packetRcvd = [];
        for (int i = 0; i < 1; i++)
        {
            packetRcvd = controller.SendReceivePacket(packet);
        }
        stopwatch.Stop();
        output.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        output.WriteLine($"len packet revc: {packetRcvd?.Length}");
        Assert.NotNull(packetRcvd);
        Assert.Equal(326, packetRcvd!.Length);
    }

    [Fact]
    public void Test_SendReceivePacket_2()
    {
        byte[] packet = [0x61, 0x00, 0x09, 0x01, 0x01, 0x31, 0xb0, 0x3b, 0x1b, 0x4f, 0xce, 0xa7, 0x00];
        Stopwatch stopwatch = new();
        stopwatch.Start();
        byte[]? packetRcvd = [];
        int n = 5;
        for (int i = 0; i < n; i++)
        {
            controller.SendPacket(packet);
        }
        for (int i = 0; i < n; i++)
        {
            packetRcvd = controller.ReceivePacket();
            output.WriteLine($"rcvd {i} {packetRcvd.Length}");
        }
        stopwatch.Stop();
        output.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        output.WriteLine($"len packet revc: {packetRcvd?.Length}");
        Assert.NotNull(packetRcvd);
        Assert.Equal(326, packetRcvd!.Length);
    }

    [Fact]
    public void Test_Sound()
    {
        var fs = 8000;
        var duration = 5.0; // seconds
        var samples = (int)(fs * duration);
        var sine = new float[samples];

        // 1) Generate 440 Hz tone
        for (int n = 0; n < samples; n++)
        {
            sine[n] = 0.5f * (float)Math.Sin(2 * Math.PI * 440 * n / fs);
        }

        Console.WriteLine("Playing original sine wave");
        PlayAudio(sine, fs);


        //controller.Reset();
        //controller.SetBlockMode();
        output.WriteLine("Product ID: " + controller.GetProductId());
        output.WriteLine("Version: " + controller.GetVersion());

        // 2) Encode blocks to AMBE
        output.WriteLine("Encoding to AMBE blocks");
        var ambeBlocks = new List<byte[]>();

        for (int i = 0; i < samples; i += 160)
        {
            var block = sine.Skip(i).Take(160).ToArray();
            if (block.Length < 160)
            {
                Array.Resize(ref block, 160);
            }

            // Convert float [-1, 1] to Int16
            var pcmShort = new short[160];
            for (int j = 0; j < 160; j++)
            {
                var v = block[j];
                v = Math.Clamp(v, -1.0f, 1.0f);
                pcmShort[j] = (short)(v * 32767);
            }

            var ambe = controller.Encode(pcmShort); // returns 7 bytes
            ambeBlocks.Add(ambe);
        }

        // 3) Decode back to PCM
        output.WriteLine("Decoding back to PCM");
        var outPcm = new List<short>();

        foreach (var ambe in ambeBlocks)
        {
            var decoded = controller.Decode(ambe, ambe.Length);
            outPcm.AddRange(decoded);
        }

        // Convert back to float for playback
        var outFloat = outPcm.Select(x => x / 32768.0f).ToArray();

        // 4) Play round-trip result
        output.WriteLine("Playing round-trip result");
        PlayAudio(outFloat, fs);

        output.WriteLine("Done!");
    }

    static void PlayAudio(float[] audioData, int sampleRate)
    {
        using var waveOut = new WaveOutEvent();
        var provider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1))
        {
            BufferDuration = TimeSpan.FromSeconds(10),
            DiscardOnBufferOverflow = true
        };

        waveOut.Init(provider);
        waveOut.Play();

        var buffer = new byte[audioData.Length * sizeof(float)];
        Buffer.BlockCopy(audioData, 0, buffer, 0, buffer.Length);
        provider.AddSamples(buffer, 0, buffer.Length);

        // Wait for duration of audio
        float durationSec = audioData.Length / (float)sampleRate;
        Thread.Sleep((int)(durationSec * 1000) + 100);  // Add a buffer margin

        waveOut.Stop(); // <- Required to exit Playing state
    }

    [Fact]
    public void Test_Sound_2()
    {
        int BlockSize = 160;

        string wavPath = "IC705_audiorecording_2025-06-14T084004_PCM.wav";
        using var reader = new WaveFileReader(wavPath);

        if (reader.WaveFormat.SampleRate != 8000 || reader.WaveFormat.Channels != 1 || reader.WaveFormat.BitsPerSample != 16)
            throw new Exception("Invalid WAV format (must be 16-bit mono 8kHz)");

        output.WriteLine($"WAV file read: {wavPath}");
        byte[] rawBytes = new byte[reader.Length];
        reader.Read(rawBytes, 0, rawBytes.Length);

        short[] pcmSamples = new short[rawBytes.Length / 2];
        Buffer.BlockCopy(rawBytes, 0, pcmSamples, 0, rawBytes.Length);

        List<short[]> pcmBlocks = [];
        for (int i = 0; i < pcmSamples.Length; i += BlockSize)
        {
            short[] block = new short[BlockSize];
            int remaining = Math.Min(BlockSize, pcmSamples.Length - i);
            Array.Copy(pcmSamples, i, block, 0, remaining);
            pcmBlocks.Add(block);
        }

        output.WriteLine($"Extracted {pcmBlocks.Count} PCM blocks");

        List<short[]> decodedBlocks = [];

        output.WriteLine("PCM->AMBE / AMBE->PCM ");
        int count = 1;
        foreach (var block in pcmBlocks)
        {
            byte[] ambe = controller.Encode(block);     // PCM -> AMBE
            Thread.Sleep(10);
            short[] pcm = controller.Decode(ambe, ambe.Length); // AMBE -> PCM
            decodedBlocks.Add(pcm);
            output.WriteLine($"\rPCM block: {count++}");
        }

        output.WriteLine("\nPlay the final PCM data ...");
        //float[] allSamples = decodedBlocks.SelectMany(b => b).ToArray();
        //PlayAudio(allSamples, 8000);

    }
}