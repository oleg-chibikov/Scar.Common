using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters.Image;

/// <summary>Converts make and model into a string value (e.g. Google - Pixel 7 Pro).</summary>
public sealed class CameraConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        _ = values ?? throw new ArgumentNullException(nameof(values));

        return values[0] == null || values[1] == null || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue
            ? string.Empty
            : $"{values[0]} - {values[1]}";
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        var str = value as string;
        if (string.IsNullOrEmpty(str))
        {
            return new object[2];
        }

        return str.Split(" - ").Cast<object>().ToArray();
    }
}
