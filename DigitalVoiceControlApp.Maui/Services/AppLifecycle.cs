using NLog;

namespace DigitalVoiceControlApp.Maui.Services;

/// <summary>
/// Meldet, wann die App den Vordergrund verliert bzw. zurückkehrt. Auf Android kommt das aus
/// <c>MainActivity.OnPause()</c>/<c>OnResume()</c>: <c>OnPause</c> wird SOFORT aufgerufen, wenn der Benutzer
/// Home oder die Übersicht (Viereck-Symbol) drückt, den Bildschirm ausschaltet oder eine andere App/ein
/// System-Dialog den Fokus übernimmt. <c>OnStop</c> käme dagegen verzögert.
/// </summary>
public static class AppLifecycle
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static volatile bool _inForeground = true;

    /// <summary>true, solange die App im Vordergrund ist und den Fokus hat.</summary>
    public static bool IsInForeground => _inForeground;

    /// <summary>Wird auf dem UI-Thread ausgelöst, sobald die App den Vordergrund verliert. Handler müssen schnell zurückkehren.</summary>
    public static event Action? Pausing;

    /// <summary>Wird ausgelöst, wenn die App wieder im Vordergrund ist.</summary>
    public static event Action? Resumed;

    internal static void RaisePausing()
    {
        _inForeground = false;
        Log.Debug("App paused");
        try { Pausing?.Invoke(); }
        catch (Exception ex) { Log.Error(ex, "Exception in a Pausing handler."); }
    }

    internal static void RaiseResumed()
    {
        _inForeground = true;
        Log.Debug("App resumed");
        try { Resumed?.Invoke(); }
        catch (Exception ex) { Log.Error(ex, "Exception in a Resumed handler."); }
    }

    /// <summary>
    /// Wartet, bis die App im Vordergrund ist (z.B. nachdem ein System-Dialog sie kurz pausiert hat).
    /// </summary>
    /// <returns>true, wenn die App innerhalb der Zeit im Vordergrund war.</returns>
    public static async Task<bool> WaitForForegroundAsync(TimeSpan timeout)
    {
        DateTime end = DateTime.UtcNow + timeout;
        while (!_inForeground && DateTime.UtcNow < end)
            await Task.Delay(50);
        return _inForeground;
    }
}
