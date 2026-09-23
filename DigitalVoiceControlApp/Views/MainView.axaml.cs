using Avalonia.Controls;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoice.Nxdn;
using DigitalVoiceControlApp.Config;
using NLog;
using System.Globalization;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalVoiceControlApp.Views;


public partial class MainView : UserControl
{ 
    readonly static NLog.Logger logger = LogManager.GetCurrentClassLogger();

    readonly SpeechSynthesizer _synthesizer = new();

    bool _textToSpeech = false;

    /// <summary>
    /// Constructor of this MainView class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        // Load User Repository (for DMR)
        DmrUserRepository.LoadData(Path.Combine(UserSettings.Dir(Mode.Dmr, UserSettings.FileType.Data), "user.csv"));

        // Load User Repository (for NXDN)
        NxdnUsers.LoadData(Path.Combine(UserSettings.Dir(Mode.Nxdn, UserSettings.FileType.Data), "nxdn.csv"));

        // Load DMR talksgroups provided by the user
        string filePath = Path.Combine(UserSettings.Dir(Mode.Dmr, UserSettings.FileType.Data), "DmrTalkGroups.csv");
        DmrTalkgroups.LoadData(filePath);

        UserSettings us = UserSettings.Instance();

        // Voice 
        foreach (var voice in _synthesizer.GetInstalledVoices())
        {
            var info = voice.VoiceInfo;
            if (info.Culture.TwoLetterISOLanguageName == "en")
            {
                _synthesizer.SelectVoice(info.Name);
                break;
            }
        }

        Thread.CurrentThread.CurrentUICulture = new CultureInfo(us.Common.Language);
    }

    public void OnAppClosing()
    {
        // Your cleanup logic
        logger.Debug("");
    }

    public async Task SpeakAsync(string text)
    {
        if (!_textToSpeech) return;

        var tcs = new TaskCompletionSource<bool>();

        void Handler(object? sender, SpeakCompletedEventArgs e)
        {
            _synthesizer.SpeakCompleted -= Handler;
            tcs.SetResult(true);
        }

        _synthesizer.SpeakCompleted += Handler;
        _synthesizer.SpeakAsync(text);

        await tcs.Task;
    }
}