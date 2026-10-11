namespace DigitalVoice.AmbeSupport;

/// <summary>Art der Verbindung eines seriellen Ports.</summary>
public enum SerialPortKind
{
    /// <summary>USB-Seriell-Gerät (zum Beispiel FTDI, CDC). So hängt der AMBE-Stick am Rechner.</summary>
    Usb,

    /// <summary>Port einer Bluetooth-Verbindung (SPP). Für den AMBE-Stick nicht von Bedeutung.</summary>
    Bluetooth,

    /// <summary>Alles andere (eingebaute Schnittstelle, PCI-Karte, Seriell-über-LAN der Intel-Verwaltung und so weiter).</summary>
    Other,
}

/// <summary>
/// Ein vorhandener serieller Port mit den Angaben, die eine Auswahlliste braucht.
/// </summary>
/// <param name="PortName">Name zum Öffnen: Windows "COM13", Linux "/dev/ttyUSB0".</param>
/// <param name="Description">Bezeichnung des Geräts (Windows: Name aus dem Geräte-Manager, Linux: Hersteller und Produkt).</param>
/// <param name="Kind">Art der Verbindung.</param>
/// <param name="VendorId">USB-Vendor-ID, falls bekannt (FTDI: 0x0403).</param>
/// <param name="ProductId">USB-Product-ID, falls bekannt (FT230X: 0x6015).</param>
public sealed record SerialPortInfo(string PortName, string Description, SerialPortKind Kind, int? VendorId = null, int? ProductId = null)
{
    /// <summary>Wert für "den Stick automatisch suchen" (statt eines festen Ports).</summary>
    public const string Auto = "AUTO";

    /// <summary>Eintrag für die Auswahlliste: der Stick wird beim Verbinden automatisch gesucht.</summary>
    public static SerialPortInfo AutoDetect { get; } = new(Auto, "detect the stick automatically", SerialPortKind.Other);

    /// <summary>Text für die Auswahlliste, zum Beispiel <c>COM13 – USB Serial Port (0403:6015)</c>.</summary>
    public string DisplayName
    {
        get
        {
            string ids = VendorId is int v && ProductId is int p ? $" ({v:X4}:{p:X4})" : "";
            return string.IsNullOrWhiteSpace(Description) ? PortName : $"{PortName} – {Description}{ids}";
        }
    }

    public override string ToString() => DisplayName;
}
