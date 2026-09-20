using Avalonia.Controls;
using DigitalVoiceControlApp.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace DigitalVoiceControlApp.Services;

public class SettingsService()
{
    public static async Task<bool> ShowSettingsDialogAsync(Window owner)
    {
        var dialog = new SettingsDialog();
        return await dialog.ShowDialog<bool>(owner);
    }

    public static void ShowAboutDialog(Window owner)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("About", $"Digital Voice Control App\nVersion 1.0.0\nde Hans (DL1HGB)",
            ButtonEnum.Ok, windowStartupLocation: WindowStartupLocation.CenterOwner);
        if (owner is not null)
             box.ShowWindowDialogAsync(owner);
    }
}

