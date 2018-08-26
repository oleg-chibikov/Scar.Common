using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}