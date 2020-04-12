using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(double))]
    public class PercentageIntToDoubleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (int)(value ?? 0) / 100d;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (int)(value ?? 0) * 100;
        }
    }
}
