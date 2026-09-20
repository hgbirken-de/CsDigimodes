using Avalonia.Controls;
using DigitalVoiceControlApp.ViewModels;

namespace DigitalVoiceControlApp.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();
        DataContext = new SettingsDialogModel();
    }

}