using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters;

[ValueConversion(typeof(double), typeof(string))]
public sealed class DoubleToPercentageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = (double)(value ?? 0);

        // Multiply by 100 and format as a percentage
        return (val * 100).ToString("#.#", CultureInfo.InvariantCulture) + "%";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
