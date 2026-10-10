using Avalonia.Controls;
using DigitalVoice.SoftwareVocoder;
using DigitalVoiceControlApp.ViewModels;
using DigitalVoiceControlApp.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DigitalVoiceControlApp.Services;

public class SettingsService()
{
    public static async Task<bool> ShowSettingsDialogAsync(Window owner)
    {
        var dialog = new SettingsDialog();
        return await dialog.ShowDialog<bool>(owner);
    }

    public static async Task<bool> ShowTgEditorDialogAsync(Window owner)
    {
        var dialog = new TalkgroupEditorDialog();
        return await dialog.ShowDialog<bool>(owner);
    }

    public static void ShowAboutDialog(Window owner)
    {
        Assembly asmVocoder = typeof(AmbeSoftwareDecoder).Assembly;   // DigitalVoice.SoftwareVocoder
        string[] thirdParty = asmVocoder.GetCustomAttributes<AssemblyMetadataAttribute>().Where(m => m.Key == "ThirdParty").Select(m => m.Value ?? "").ToArray();
        string includes = thirdParty.Length > 0 ? "Includes:\n" + string.Join("\n", thirdParty.Select(t => "  " + t)) : "";

        Assembly asmApp = Assembly.GetEntryAssembly() ?? typeof(MainViewModel).Assembly;
        string product = asmApp.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "HamDigiModes";
        string authors = asmApp.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "";
        string description = asmApp.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
        string copyright = asmApp.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";
        string? license = asmApp.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == "License")?.Value;
        string? projectUrl = asmApp.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(m => m.Key == "ProjectUrl")?.Value;

        string version = asmApp.GetName().Version?.ToString(3) ?? "?";
        string info = asmApp.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
        int plus = info.IndexOf('+');
        string git = plus >= 0 && plus < info.Length - 1 ? info[(plus + 1)..Math.Min(info.Length, plus + 10)] : "";

        string text =
            $"{product} - {description}\n\n" +
            //$"Author {authors}\n" +
            $"Version: {version}" + (git.Length > 0 ? $" (git {git})" : "") + "\n" +
            $"{copyright}\n\n" +
            $"License: {license}\n\n" +
            $"Source: {projectUrl}\n\n" +
            $"{includes}\n\n" +
            "Full license texts: LICENSE-GPL.txt and LICENSE-mbelib.txt in the installation folder.";

        var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ContentTitle = $"About {product}",
            ContentMessage = text,
            ButtonDefinitions = ButtonEnum.Ok,
            MaxWidth = 500,                                   // Zeilen werden darüber umgebrochen
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        });

        if (owner is not null)
            box.ShowWindowDialogAsync(owner);
    }

    public static void ShowAlertDialog(Window owner, string? title, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
        {
            ContentTitle = title ?? "Alert",
            ContentMessage = message,
            ButtonDefinitions = ButtonEnum.Ok,
            //MaxWidth = 500,                                   // Zeilen werden darüber umgebrochen
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        });

        if (owner is not null)
            box.ShowWindowDialogAsync(owner);
    }
}
