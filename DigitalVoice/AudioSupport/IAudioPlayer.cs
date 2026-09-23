namespace DigitalVoice.AudioSupport;

public interface IAudioPlayer
{
    float GainDb { get; set; }

    void FeedPcmData(byte[] pcmData, int offset = 0, int count = -1);
}
