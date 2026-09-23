using Avalonia;
using Avalonia.Controls;
using DigitalVoiceControlApp.Config;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;

namespace DigitalVoiceControlApp.Views;

public partial class MainWindow : Window
{
    static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
    public MainWindow()
    {
        InitializeComponent();
        UserSettings us = UserSettings.Instance();
        Position = new PixelPoint(us.Gui.FrameXPos, us.Gui.FrameYPos);

        Closing += WindowClosing;
    }

    private bool _exitInProgress = false;

    private async void WindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        logger.Debug($"_exitInProgress={_exitInProgress}");
        if (_exitInProgress) return;
        try
        {
            _exitInProgress = true;
            e.Cancel = true; // because we either close manually or cancel closing
            var msgBox = MessageBoxManager.GetMessageBoxStandard("Confirm", "Are you sure you want to exit?", ButtonEnum.YesNo,
                        windowStartupLocation: WindowStartupLocation.CenterOwner);
            var result = await msgBox.ShowAsync();
            if (result == ButtonResult.Yes)
            {
                if (Content is MainView mainView)
                    mainView.OnAppClosing();

                // position on screen
                UserSettings us = UserSettings.Instance();
                us.Gui.FrameXPos = Position.X;
                us.Gui.FrameYPos = Position.Y;
                us.Gui.FrameHeight = Height;
                us.Gui.FrameWidth = Width;
                UserSettings.Save();

                Closing -= WindowClosing; // never come back here
                Close();
                //logger.Debug($"_currTxType = {_currTxType}, _exitInProgress={_exitInProgress}");
            }
        }
        finally
        {
            _exitInProgress = false; // reentrancy guard (if user clicks NO)
        }
    }
}