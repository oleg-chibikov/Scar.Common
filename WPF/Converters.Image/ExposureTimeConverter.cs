using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters.Image;

/// <summary>Converts an exposure time from a decimal (e.g. 0.0125) into a string (e.g. 1/80).</summary>
public sealed class ExposureTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null ? null : $"1/{Math.Round(1 / (double)value)}";
    }

    public object? ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture)
    {
        return value is not string str ? null : 1 / decimal.Parse(str[2..], CultureInfo.InvariantCulture);
    }
}
