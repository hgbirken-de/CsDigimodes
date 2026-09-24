using DigitalVoice.DStar.Common;
using System.Text;
using Xunit.Abstractions;

namespace DigitalVoiceTest;

public class UnitTest_DStar_Helpers(ITestOutputHelper output)
{
    readonly ITestOutputHelper output = output;

    [Fact]
    public void Test_CRC()
    {
        string[] gpsData = [
            "$$CRC880B,DO3MAC-4>API52,DSTAR*:/251544z5117.92N/00720.67E[120/000/A=00065273 Martin, ICOM ID-52" + "\r",
            "$$CRCA48E,DO3MAC-4>API52,DSTAR*:/251544z5117.92N/00720.67E[120/000/A=00065473 Martin, ICOM ID-52" + "\r",
            "$$CRC38D6,DO3MAC-4>API52,DSTAR*:/251544z5117.93N/00720.68E[015/003/A=00064273 Martin, ICOM ID-52" + "\r",
            "$$CRCCE3E,AE5PL-T>API282,DSTAR*:!3302.39N/09644.66W>/" + "\r", // from https://www.aprs-is.net/images/D-PRS.pdf#page4, Peter Loveall AE5PL
            
            ];

        foreach (var gd in gpsData)
        {
            string parsedCrc = gd[5..9];
            int crcCalculated = Helpers.CalcCcittCrc(Encoding.ASCII.GetBytes(gd), 10, gd.Length - 10);
            output.WriteLine($"0x{parsedCrc} {crcCalculated} 0x{crcCalculated:X4}");
            Assert.Equal(parsedCrc, crcCalculated.ToString("X4"));
        }
    }
}