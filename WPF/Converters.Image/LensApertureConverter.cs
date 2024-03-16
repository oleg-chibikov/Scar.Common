using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters.Image;

/// <summary>Converts a lens aperture from a decimal into a human-preferred string (e.g. 1.8 becomes F1.8).</summary>
public sealed class LensApertureConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null ? $"F{value:##.0}" : null;
    }

    public object? ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture)
    {
        var str = value as string;
        return !string.IsNullOrEmpty(str) ? decimal.Parse(str[1..], CultureInfo.InvariantCulture) : null;
    }
}
