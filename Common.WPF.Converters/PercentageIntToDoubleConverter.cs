using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(double))]
    public class PercentageIntToDoubleConverter : IValueConverter
    {
        [NotNull]
        public object Convert(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            return (int)(value ?? 0) / 100d;
        }

        [NotNull]
        public object ConvertBack(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            return (int)(value ?? 0) * 100;
        }
    }
}