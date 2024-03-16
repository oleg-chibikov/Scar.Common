using System;
using System.Globalization;
using System.Windows.Data;
using Scar.Common.ImageProcessing.Metadata;

namespace Scar.Common.WPF.Converters.Image;

/// <summary>
/// Converts a GeoLocation object into a human-readable string.
/// </summary>
public class GeoLocationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GeoLocation geoLocation)
        {
            return geoLocation.Name ?? $"{FormatCoordinate(geoLocation.Latitude)}, {FormatCoordinate(geoLocation.Longitude)}";
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    static string FormatCoordinate(double? coordinate) => coordinate.HasValue ? $"{coordinate.Value}" : "N/A";
}
