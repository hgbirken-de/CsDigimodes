using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace DigitalVoiceControlApp.Converters;

/// <summary>
/// Liefert zu einem Icon-Basisnamen (ConverterParameter, z.B. "AudioRecording") die zum Theme
/// passende PNG-Variante: Light = "AudioRecording.png", Dark = "AudioRecording_dark.png".
/// Binding-Quelle ist <c>ActualThemeVariant</c> des Controls - dadurch aktualisiert sich das Icon
/// automatisch, sobald das Theme umgeschaltet wird.
/// </summary>
public class ThemeIconConverter : IValueConverter
{
    static readonly ConcurrentDictionary<string, Bitmap> _cache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string baseName) return null;

        bool dark = ThemeVariant.Dark.Equals(value);
        string file = dark ? $"{baseName}_dark.png" : $"{baseName}.png";

        return _cache.GetOrAdd(file, f => new Bitmap(AssetLoader.Open(new Uri($"avares://DigitalVoiceControlApp/Assets/{f}"))));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
