using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// Ein Eintrag der Last-Heard-Liste für D-STAR (DCS, REF, XRF). Die Zeile ist wie in der Desktop-App: Zeit, Gateway (RPT1),
/// Quelle (MYCALL), Ziel (URCALL). Gateway und Ziel werden bei Bedarf mit den folgenden Frames nachgetragen
/// (<see cref="Update"/>); die Zeit der Zeile bleibt.
/// </summary>
public class LastHeardItemDStar : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private readonly string _time;

    public string Src { get; }
    public string Dst { get; private set; }
    public string Gw { get; private set; }

    /// <summary>Der Reflektor mit Modul (z.B. DCS001C), wie ihn der Client meldet.</summary>
    public string Type { get; }

    /// <summary>Die Anzeigezeile. Daran bindet die Liste.</summary>
    public string Display { get; private set; }

    public LastHeardItemDStar(string dst, string gw, string src, string type)
    {
        _time = DateTime.Now.ToString("HH:mm:ss MMM/dd", CultureInfo.InvariantCulture);
        Dst = dst;
        Gw = gw;
        Src = src;
        Type = type;
        Display = BuildDisplay();
    }

    private string BuildDisplay() => $"{_time},{Gw},{Src},{Dst}";

    /// <summary>Trägt ein später bekannt gewordenes Gateway bzw. Ziel nach. Leere Werte ändern nichts.</summary>
    /// <returns><c>true</c>, wenn sich die Zeile geändert hat.</returns>
    public bool Update(string gw, string dst)
    {
        bool changed = false;

        if (gw.Length > 0 && gw != Gw)
        {
            Gw = gw;
            changed = true;
        }
        if (dst.Length > 0 && dst != Dst)
        {
            Dst = dst;
            changed = true;
        }

        if (changed)
        {
            Display = BuildDisplay();
            OnPropertyChanged(nameof(Display));
        }

        return changed;
    }

    public override string ToString() => Display;
}
