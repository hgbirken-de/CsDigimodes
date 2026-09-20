
using DigitalVoice.Dmr;
using Xunit.Abstractions;

namespace YsfClientTest;

public class UnitTest_DmrUserInfoReader(ITestOutputHelper output)
{
    readonly ITestOutputHelper output = output;

    [Fact]
    public async Task Test_DmrUserInfoReader()
    {
        try
        {
            var result = await DmrUserDataReader.GetUserAsync(2622363);
            output.WriteLine($"{result}");
        }
        catch (Exception ex)
        {
            output.WriteLine($"{ex}");
        }
    }
}