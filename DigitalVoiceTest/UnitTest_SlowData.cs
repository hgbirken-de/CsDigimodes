using DigitalVoice.DStar.Common;
using Xunit.Abstractions;

namespace DigitalVoiceTest;

public class UnitTest_SlowData(ITestOutputHelper output)
{
    readonly ITestOutputHelper output = output;

    [Fact]
    public void Test_ParseGpsData1()
    {
        string gpsData = "$GPGGA,115615.00,4825.30,N,00901.77,E,1,03,2309.4,443.8,M,48.0,M,,*53";
        output.WriteLine(gpsData);
        string result = SlowData.ParseGpsData(gpsData);
        output.WriteLine(result);
        Assert.Equal("N48 25.30 E009 01.77 443.8m", result);
    }

    [Fact]
    public void Test_ParseGpsData2()
    {
        string gpsData = "$$CRCD578,DO3MAC-4>API52,DSTAR0635z5117.93N/00720.68E[001/001/A=000703";

        output.WriteLine(gpsData);
        string result = SlowData.ParseGpsData(gpsData);
        output.WriteLine(result);
        Assert.Equal("N51 17.93 E007 20.68 /A=000703", result);
    }

    [Fact]
    public void Test_ParseGpsData3()
    {
        string gpsData = @"$$CRCFEEE,IN3IJJ-1>API710,DSTA!4565.12N\01.152E2789GAIS"; // watch \0 in GPS data 
        output.WriteLine(gpsData);
        string result = SlowData.ParseGpsData(gpsData);
        output.WriteLine(result);
        Assert.Equal("N45 65.12 E01.152", result);
    }
}