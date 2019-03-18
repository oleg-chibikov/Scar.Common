using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(int))]
    public class AddValueConverter : IValueConverter
    {
        public object Convert(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            _ = parameter ?? throw new ArgumentNullException(nameof(parameter));
            var result = value;
            if (value != null && int.TryParse((string)parameter, NumberStyles.Integer, culture, out var parameterValue))
            {
                result = (int)value + parameterValue;
            }

            return result;
        }

        public object ConvertBack(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}