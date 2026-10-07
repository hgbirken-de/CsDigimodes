using DigitalVoiceControlApp.Maui.Services;
using NLog;

namespace DigitalVoiceControlApp.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppLogging.Initialize(); // Logging einrichten (Level, Datei, Rollover)

        var logger = LogManager.GetCurrentClassLogger();
        logger.Info("App started");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}
