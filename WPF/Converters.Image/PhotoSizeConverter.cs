using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters.Image;

/// <summary>Converts an x,y size pair into a string value (e.g. 1600x1200).</summary>
public sealed class PhotoSizeConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        _ = values ?? throw new ArgumentNullException(nameof(values));

        return values[0] == null || values[1] == null || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue
            ? string.Empty
            : $"{values[0]}x{values[1]}";
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        var str = value as string;
        if (string.IsNullOrEmpty(str))
        {
            return new object[2];
        }

        var sSize = str.Split('x');

        var size = new object[2];
        size[0] = uint.Parse(sSize[0], CultureInfo.InvariantCulture);
        size[1] = uint.Parse(sSize[1], CultureInfo.InvariantCulture);
        return size;
    }
}
