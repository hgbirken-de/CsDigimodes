using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace DigitalVoiceControlApp.Converters;

public class EnumToUpperConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value?.ToString()?.ToUpperInvariant() ?? string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Enum.Parse(targetType, value?.ToString() ?? string.Empty, ignoreCase: true);
}

