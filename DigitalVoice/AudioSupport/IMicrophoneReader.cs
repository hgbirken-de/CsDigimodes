namespace DigitalVoice.AudioSupport;

public interface IMicrophoneReader : IDisposable
{
    event EventHandler<byte[]>? AudioAvailable;

    float GainDb { get; set; }

    void Start();
    void Stop();

    int Read(byte[] buffer, int offset, int count);
    bool TryRead(byte[] buffer, int offset, int count, out int bytesRead);
}
