
using System.Management;
using System.Text.RegularExpressions;

namespace DigitalVoice.AmbeSupport;

public static class SerialPortFinder
{
    /// <summary>
    /// Sucht den COM-Port-Namen eines USB-Geräts anhand von Vendor-ID und Product-ID.
    /// </summary>
    /// <param name="vendorId">Vendor-ID, z.B. 0x0403 für FTDI.</param>
    /// <param name="productId">Product-ID, z.B. 0x6015 für FT230X.</param>
    /// <returns>Der COM-Port-Name (z.B. "COM13"), oder null falls kein passendes Gerät gefunden wurde.</returns>
    public static string? FindComPort(int vendorId, int productId)
    {
        string vid = $"VID_{vendorId:X4}";
        string pid = $"PID_{productId:X4}";

        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'");

        foreach (ManagementObject device in searcher.Get())
        {
            string? deviceId = device["PNPDeviceID"]?.ToString();
            string? caption = device["Caption"]?.ToString();

            if (deviceId != null && caption != null &&
                deviceId.Contains(vid, StringComparison.OrdinalIgnoreCase) &&
                deviceId.Contains(pid, StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(caption, @"\(COM\d+\)");
                if (match.Success)
                    return match.Value.Trim('(', ')');
            }
        }

        return null;
    }
}
