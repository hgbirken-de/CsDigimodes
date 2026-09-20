using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace DigitalVoiceControlApp.Converters;

/// <summary>
/// A value converter for Avalonia data bindings that displays integers as strings,
/// but renders <c>0</c> as an empty string. When converting back, empty or whitespace 
/// input is mapped to <c>0</c>, and valid numeric text is parsed into an integer.
/// Useful for text boxes where <c>0</c> should appear blank.
/// </summary>
// <UserControl xmlns:local="clr-namespace:YourNamespace">
//    <UserControl.Resources>
//        <local:ZeroToEmptyStringConverter x:Key="ZeroToEmptyStringConverter" />
//    </UserControl.Resources>
    
//    <TextBox Focusable = "False"
//             IsReadOnly="True"
//             Width="120"
//             Text="{Binding SrcId, Converter={StaticResource ZeroToEmptyStringConverter}}"/>
// </UserControl>

public class ZeroToEmptyStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i && i == 0)
            return string.Empty;
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value as string))
            return 0;
        if (int.TryParse(value as string, out var i))
            return i;
        return 0;
    }
}

