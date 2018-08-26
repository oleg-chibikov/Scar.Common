using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(double), typeof(int))]
    public sealed class DoubleToIntConverter : IValueConverter
    {
        [NotNull]
        public object Convert(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            return (int)Math.Ceiling((double)(value ?? 0));
        }

        [NotNull]
        public object ConvertBack(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            return System.Convert.ToDouble((int)(value ?? 0));
        }
    }
}