
namespace YsfClientTest;

using DigitalVoiceControlApp;

public class UnitTest_Usersettings
{
    [Fact]
    public void Test_01()
    {
        var instance = UserSettings.Instance();
        instance.Common.Name = "Hans";
        UserSettings.Save();
    }
}