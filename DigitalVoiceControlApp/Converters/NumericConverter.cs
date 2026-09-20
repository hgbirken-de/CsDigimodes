using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace DigitalVoiceControlApp.Converters;

public class NumericConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string r =  value?.ToString() ?? string.Empty;
        return r;
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            s = s.Replace("x", "");
            if (int.TryParse(s, out var result))
            {
                return result;
            }
            // reject invalid input → keeps old value
            return Avalonia.Data.BindingOperations.DoNothing;
        }


        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
