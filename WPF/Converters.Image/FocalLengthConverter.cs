using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters.Image;

/// <summary>Converts a focal length from a decimal into a human-preferred string (e.g. 85 becomes 85mm).</summary>
public sealed class FocalLengthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null ? $"{value}mm" : null;
    }

    public object? ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
