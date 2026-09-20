using Avalonia.Controls;
using Avalonia.Input;
using DigitalVoiceControlApp.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TextCopy;

namespace DigitalVoiceControlApp.Views;

public partial class NxdnModeControl : UserControl
{
    public NxdnModeControl()
    {
        InitializeComponent();
    }

    public void ListBoxLastHeard_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (_listBoxLastHeard.SelectedItem is LastHeardItemNxdn item)
        {
            ClipboardService.SetText(item.Callsign);
            string url = $"http://www.qrz.com/db/{item.Callsign}";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }

    private async void ListBoxLastHeard_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Avalonia.Input.Key.C)
        {
            if (_listBoxLastHeard.SelectedItem is LastHeardItemNxdn entry)
            {
                // Get the TopLevel (Window) hosting the ListBox
                var top = TopLevel.GetTopLevel(_listBoxLastHeard);
                if (top?.Clipboard != null)
                {
                    await top.Clipboard.SetTextAsync(entry.ToString());
                }
            }
        }
    }

}