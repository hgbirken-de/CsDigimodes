using System.Globalization;
using System.Management;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace DigitalVoice.AmbeSupport;

/// <summary>
/// Sucht den seriellen Port eines USB-Geräts (zum Beispiel des AMBE-Sticks) anhand von Vendor-ID und Product-ID.
/// Windows: über WMI (<c>Win32_PnPEntity</c>), Ergebnis z. B. <c>COM13</c>.
/// Linux: über <c>/sys/class/tty</c> (<c>ttyUSB*</c> und <c>ttyACM*</c>), Ergebnis z. B. <c>/dev/ttyUSB0</c>. Zum Öffnen braucht der Benutzer unter Linux
/// die Gruppe "dialout" (Debian, Ubuntu) oder "uucp" (Arch).
/// Andere Systeme: kein Ergebnis (<c>null</c>).
/// <see cref="GetPorts"/> liefert alle vorhandenen Ports mit Beschreibung für eine Auswahlliste.
/// </summary>
public static class SerialPortFinder
{
    /// <summary>
    /// Sucht den Port eines USB-Geräts anhand von Vendor-ID und Product-ID.
    /// </summary>
    /// <param name="vendorId">Vendor-ID, z.B. 0x0403 für FTDI.</param>
    /// <param name="productId">Product-ID, z.B. 0x6015 für FT230X.</param>
    /// <returns>Der Port-Name (Windows: "COM13", Linux: "/dev/ttyUSB0"), oder null, falls kein passendes Gerät gefunden wurde.</returns>
    public static string? FindComPort(int vendorId, int productId)
    {
        if (OperatingSystem.IsWindows())
            return FindOnWindows(vendorId, productId);

        if (OperatingSystem.IsLinux())
            return FindOnLinux(vendorId, productId);

        return null;
    }

    /// <summary>
    /// Liefert die wirklich vorhandenen seriellen Ports (Windows: was der Geräte-Manager unter "Anschlüsse (COM &amp; LPT)" zeigt,
    /// Linux: <c>ttyUSB*</c>, <c>ttyACM*</c> und <c>rfcomm*</c>), sortiert nach Portnummer.
    /// Für die Auswahlliste im Einstellungsdialog statt einer festen Liste COM1 bis COM20.
    /// </summary>
    /// <param name="includeBluetooth"><c>false</c> (Vorgabe) blendet Bluetooth-Ports aus: Der AMBE-Stick hängt per USB.</param>
    /// <param name="includeOther"><c>false</c> zeigt nur USB-Ports.</param>
    public static IReadOnlyList<SerialPortInfo> GetPorts(bool includeBluetooth = false, bool includeOther = true)
    {
        List<SerialPortInfo> ports =
            OperatingSystem.IsWindows() ? GetPortsOnWindows() :
            OperatingSystem.IsLinux() ? GetPortsOnLinux() :
            [];

        return ports
            .Where(p => p.Kind switch
            {
                SerialPortKind.Bluetooth => includeBluetooth,
                SerialPortKind.Other => includeOther,
                _ => true,
            })
            .OrderBy(p => PortNumber(p.PortName))
            .ThenBy(p => p.PortName, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Zahl am Ende des Namens ("COM13" → 13, "/dev/ttyUSB2" → 2), damit COM9 vor COM10 steht.</summary>
    private static int PortNumber(string portName)
    {
        int end = portName.Length;
        int start = end;
        while (start > 0 && char.IsDigit(portName[start - 1]))
            start--;

        return start < end && int.TryParse(portName[start..end], out int n) ? n : int.MaxValue;
    }

    // ------------------------------------------------------------------
    // Windows
    // ------------------------------------------------------------------

    [SupportedOSPlatform("windows")]
    private static List<SerialPortInfo> GetPortsOnWindows()
    {
        var ports = new List<SerialPortInfo>();
        using var searcher = new ManagementObjectSearcher("SELECT Caption, PNPDeviceID FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'");

        foreach (ManagementObject device in searcher.Get())
        {
            SerialPortInfo? info = ParseWindowsEntry(device["PNPDeviceID"]?.ToString(), device["Caption"]?.ToString());
            if (info != null)
                ports.Add(info);
        }

        return ports;
    }

    /// <summary>
    /// Macht aus einem Eintrag des Geräte-Managers (Name und Geräte-Kennung) einen <see cref="SerialPortInfo"/>. Die Art ergibt sich aus
    /// dem Anfang der Kennung: <c>USB\</c> und <c>FTDIBUS\</c> = USB, <c>BTHENUM\</c> und <c>BTH\</c> = Bluetooth, sonst anderes.
    /// </summary>
    internal static SerialPortInfo? ParseWindowsEntry(string? pnpDeviceId, string? caption)
    {
        if (caption == null)
            return null;

        Match m = Regex.Match(caption, @"\((COM\d+)\)\s*$");   // der Name endet auf "(COM13)"
        if (!m.Success)
            return null;

        string id = pnpDeviceId ?? "";
        SerialPortKind kind =
            id.StartsWith("USB\\", StringComparison.OrdinalIgnoreCase) || id.StartsWith("FTDIBUS\\", StringComparison.OrdinalIgnoreCase) ? SerialPortKind.Usb :
            id.StartsWith("BTHENUM\\", StringComparison.OrdinalIgnoreCase) || id.StartsWith("BTHMODEM\\", StringComparison.OrdinalIgnoreCase) ||
            id.StartsWith("BTH\\", StringComparison.OrdinalIgnoreCase) ? SerialPortKind.Bluetooth :
            SerialPortKind.Other;

        return new SerialPortInfo(m.Groups[1].Value, caption[..m.Index].Trim(), kind, ParseHexId(id, "VID_"), ParseHexId(id, "PID_"));
    }

    private static int? ParseHexId(string text, string prefix)
    {
        Match m = Regex.Match(text, prefix + "([0-9A-Fa-f]{4})");
        return m.Success ? int.Parse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture) : null;
    }

    [SupportedOSPlatform("windows")]
    private static string? FindOnWindows(int vendorId, int productId)
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

    // ------------------------------------------------------------------
    // Linux
    // ------------------------------------------------------------------

    /// <summary>
    /// Geht <c>/sys/class/tty/ttyUSB*</c> und <c>ttyACM*</c> durch. Jeder Eintrag ist ein Verweis auf das Gerät im USB-Baum; in dessen
    /// Elternordnern liegt beim USB-Gerät selbst <c>idVendor</c> und <c>idProduct</c> (Hexadezimal, z. B. "0403").
    /// Die Ordner sind für Tests austauschbar.
    /// </summary>
    internal static string? FindOnLinux(int vendorId, int productId, string sysClassTty = "/sys/class/tty", string devDirectory = "/dev") =>
        GetPortsOnLinux(sysClassTty, devDirectory)
            .FirstOrDefault(p => p.Kind == SerialPortKind.Usb && p.VendorId == vendorId && p.ProductId == productId)?.PortName;

    /// <summary>
    /// Alle vorhandenen Ports unter Linux: <c>ttyUSB*</c> und <c>ttyACM*</c> (USB, mit Vendor-ID, Product-ID und Beschreibung aus dem USB-Gerät)
    /// sowie <c>rfcomm*</c> (Bluetooth). Eingebaute <c>ttyS*</c> werden nicht aufgeführt: Davon meldet der Kernel auch solche, die es nicht gibt.
    /// Die Ordner sind für Tests austauschbar.
    /// </summary>
    internal static List<SerialPortInfo> GetPortsOnLinux(string sysClassTty = "/sys/class/tty", string devDirectory = "/dev")
    {
        var ports = new List<SerialPortInfo>();
        if (!Directory.Exists(sysClassTty))
            return ports;

        foreach (string ttyDir in Directory.EnumerateDirectories(sysClassTty))
        {
            string name = Path.GetFileName(ttyDir);
            string portName = Path.Combine(devDirectory, name);

            if (name.StartsWith("ttyUSB", StringComparison.Ordinal) || name.StartsWith("ttyACM", StringComparison.Ordinal))
            {
                string? usbDevice = FindUsbDeviceDirectory(ttyDir);
                if (usbDevice == null)
                    continue;

                string description = $"{ReadText(Path.Combine(usbDevice, "manufacturer"))} {ReadText(Path.Combine(usbDevice, "product"))}".Trim();
                ports.Add(new SerialPortInfo(portName, description.Length > 0 ? description : "USB serial device", SerialPortKind.Usb,
                    ReadHex(Path.Combine(usbDevice, "idVendor")), ReadHex(Path.Combine(usbDevice, "idProduct"))));
            }
            else if (name.StartsWith("rfcomm", StringComparison.Ordinal))
            {
                ports.Add(new SerialPortInfo(portName, "Bluetooth serial (RFCOMM)", SerialPortKind.Bluetooth));
            }
        }

        return ports;
    }

    /// <summary>
    /// Löst den Verweis des tty-Eintrags auf und sucht in den Elternordnern den ersten, der <c>idVendor</c> und <c>idProduct</c> enthält
    /// (das ist das USB-Gerät). Liefert <c>null</c>, wenn es keinen gibt (zum Beispiel bei einem eingebauten Port).
    /// </summary>
    private static string? FindUsbDeviceDirectory(string ttyDir)
    {
        string real = ttyDir;
        try
        {
            FileSystemInfo? target = new DirectoryInfo(ttyDir).ResolveLinkTarget(returnFinalTarget: true);
            if (target != null)
                real = Path.GetFullPath(target.FullName);
        }
        catch (IOException)
        {
            return null;
        }

        DirectoryInfo? dir = new DirectoryInfo(real);
        for (int level = 0; level < 10 && dir != null; level++, dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "idVendor")) && File.Exists(Path.Combine(dir.FullName, "idProduct")))
                return dir.FullName;
        }

        return null;
    }

    private static string ReadText(string path)
    {
        try
        {
            return File.ReadAllText(path).Trim();
        }
        catch (IOException)
        {
            return "";
        }
    }

    private static int? ReadHex(string path)
    {
        try
        {
            string text = File.ReadAllText(path).Trim();
            return int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value) ? value : null;
        }
        catch (IOException)
        {
            return null;
        }
    }
}
