using System.Reflection;
using System.Runtime.InteropServices;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// Angaben der About-Seite: Name, Version, Git-Build, System, Copyright und Lizenz. Die Texte zu Copyright und Lizenz
/// stehen hier an einer Stelle, damit sie leicht geändert werden können.
/// </summary>
public sealed class AboutInfo
{
    // ------------------------------------------------------------------
    // Hier anpassen
    // ------------------------------------------------------------------

    /// <summary>Adresse der Projektseite (z.B. des Git-Repositorys). Leer = der Knopf "Project page" wird nicht angezeigt.</summary>
    public const string ProjectUrl = "";

    /// <summary>Adresse des Lizenztextes.</summary>
    public const string LicenseUrl = "https://www.gnu.org/licenses/gpl-3.0.html";

    public string AppName => ".NET Digimodes";

    public string Copyright => "Copyright © 2026 Hans-Gunther Birken, DL1HGB";

    /// <summary>Lizenzhinweis (GNU GPL Version 3 oder neuer). Bei einer anderen Lizenz hier austauschen.</summary>
    public string LicenseText =>
        "This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public " +
        "License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.\n\n" +
        "This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied " +
        "warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.\n\n" +
        "You should have received a copy of the GNU General Public License along with this program. " +
        "If not, see https://www.gnu.org/licenses/.";

    /// <summary>Lizenztext der mbelib (ISC-Lizenz). Er muss bei der Weitergabe mitgeliefert werden.</summary>
    public string IscText =>
        """
        Copyright (C) 2010 mbelib Author

        Permission to use, copy, modify, and/or distribute this software for any purpose with or without fee is hereby granted, provided that the above copyright notice and this permission notice appear in all copies.

        THE SOFTWARE IS PROVIDED "AS IS" AND ISC DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL ISC BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
        """;

    // ------------------------------------------------------------------
    // Ermittelte Angaben
    // ------------------------------------------------------------------

    public bool HasProjectUrl => !string.IsNullOrWhiteSpace(ProjectUrl);

    /// <summary>
    /// <c>true</c>, wenn die GPL-Sprachanalyse des Software-Kodierers (Projekt DigitalVoice.SoftwareVocoder.Imbe) Teil der App
    /// ist. Dann erscheint ihr Hinweis in der Liste der Fremdsoftware.
    /// </summary>
    public bool HasGplEncoder { get; } = DetectGplEncoder();

    /// <summary>Version aus dem Projekt (ApplicationDisplayVersion und ApplicationVersion).</summary>
    public string Version { get; } = $"{AppInfo.Current.VersionString} (build {AppInfo.Current.BuildString})";

    public string VersionLine => $"Version {AppInfo.Current.VersionString}";

    /// <summary>
    /// Git-Commit, aus dem die App gebaut wurde. Das .NET-SDK trägt ihn als "+hash" in die InformationalVersion der
    /// Assembly ein, wenn das Projekt in einem Git-Repository liegt.
    /// </summary>
    public string GitBuild { get; } = ReadGitBuild();

    public string Platform { get; } = ReadPlatform();

    public string Device { get; } = $"{DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model}".Trim();

    /// <summary>Architektur, für die die laufende App gebaut ist (z.B. Arm64).</summary>
    public string Architecture { get; } = RuntimeInformation.ProcessArchitecture.ToString();

    /// <summary>ABIs, die das Gerät unterstützt (Android), z.B. "arm64-v8a, armeabi-v7a".</summary>
    public string DeviceAbis { get; } = ReadDeviceAbis();

    public string Runtime { get; } = RuntimeInformation.FrameworkDescription;

    /// <summary>Alle Angaben als Text, zum Kopieren (z.B. für eine Fehlermeldung).</summary>
    public string ToClipboardText() =>
        $"{AppName}\n" +
        $"Version:\t{Version}\n" +
        $"Git build:\t{GitBuild}\n" +
        $"Platform:\t{Platform}\n" +
        $"Device:\t{Device}\n" +
        $"Architecture:\t{Architecture}\n" +
        $"Device ABIs:\t{DeviceAbis}\n" +
        $"Runtime:\t{Runtime}";

    // ------------------------------------------------------------------

    private static string ReadGitBuild()
    {
        string? info = typeof(AboutInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        int plus = info?.IndexOf('+') ?? -1;
        if (info == null || plus < 0 || plus == info.Length - 1)
            return "not available";

        string hash = info[(plus + 1)..];
        return hash.Length > 9 ? hash[..9] : hash;
    }

    private static string ReadPlatform()
    {
        string text = $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}";
#if ANDROID
        int apiLevel = (int)Android.OS.Build.VERSION.SdkInt;
        text += $" (API {apiLevel})";
#endif
        return text;
    }

    private static string ReadDeviceAbis()
    {
#if ANDROID
        IList<string>? abis = Android.OS.Build.SupportedAbis;
        if (abis != null && abis.Count > 0)
            return string.Join(", ", abis);
#endif
        return "n/a";
    }

    private static bool DetectGplEncoder()
    {
        try
        {
            return Type.GetType("DigitalVoice.SoftwareVocoder.Imbe.ImbeAnalyzer, DigitalVoice.SoftwareVocoder.Imbe") != null;
        }
        catch
        {
            return false;
        }
    }
}
