

using DigitalVoice.AudioSupport;
using Xunit.Abstractions;

namespace DigitalVoiceTest;

public class UnitTest_AudioCapture(ITestOutputHelper output)
{
    readonly ITestOutputHelper output = output;

    [Fact]
    public void Test_AudioCapture()
    {
        MicrophoneReader ac = new();
        WavPcmRecorder? recorder = new("mic_capture.wav");

        ac.Start();

        byte[] pcm = new byte[320];

        for (int i = 0; i < 500; i++)
        { 
            Thread.Sleep(20);
            
            if (ac.TryRead(pcm, 0, pcm.Length, out int bytesRead))
            {
                recorder.WritePcm(pcm);
            }
            else
            {
                output.WriteLine($"Unable to read PCM data, i={i}");
            }
        }

        ac.Stop();

        recorder.Dispose();
    }
}