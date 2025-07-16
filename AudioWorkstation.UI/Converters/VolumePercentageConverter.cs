using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace AudioWorkstation.UI.Converters;

public class VolumePercentageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float floatValue)
            return floatValue * 100;
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
            return (float)(doubleValue / 100.0);
        return 0f;
    }
}