using DigitalVoice.AmbeSupport;
using Xunit.Abstractions;

namespace DigitalVoiceTest;

public class SerialPortFinderTest(ITestOutputHelper output)
{
    readonly ITestOutputHelper output = output;
#if WINDOWS
    [Fact]
    public void FindComPort_ReturnsExpectedPort_ForKnownDevice()
    {
        string? port = SerialPortFinder.FindComPort(0x0403, 0x6015);
        output.WriteLine($"port={port}");
        Assert.NotNull(port);
        Assert.StartsWith("COM", port);
    }
#endif
}
