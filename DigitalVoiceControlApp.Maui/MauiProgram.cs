using NLog;
using NLog.Config;
using NLog.Targets;

namespace DigitalVoiceControlApp.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        InitializeLogging();

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

    private static void InitializeLogging()
    {
        var logDir = Path.Combine(FileSystem.AppDataDirectory, "logs");
        Directory.CreateDirectory(logDir);

        const string layout = "${longdate} ${uppercase:${level}} ${threadid} ${callsite}(): ${message} ${exception:format=toString,StackTrace}";

        var config = new LoggingConfiguration();

        var fileTarget = new FileTarget("file")
        {
            FileName = Path.Combine(logDir, "log-${cached:${date:format=yyyy-MM-dd_HH:mm:ss}}.log"),
            Layout = layout
        };
        var consoleTarget = new ConsoleTarget("console") { Layout = layout };
        var debuggerTarget = new DebuggerTarget("debugger") { Layout = layout };

        foreach (var pattern in new[] { "DigitalVoiceControlApp.*", "DigitalVoice.*", "Maui.*" })
        {
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget, pattern);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget, pattern);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, debuggerTarget, pattern);
        }

        LogManager.Configuration = config;
    }
}