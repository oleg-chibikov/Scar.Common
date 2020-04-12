using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(double), typeof(int))]
    public sealed class DoubleToIntConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (int)Math.Ceiling((double)(value ?? 0));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble((int)(value ?? 0));
        }
    }
}
