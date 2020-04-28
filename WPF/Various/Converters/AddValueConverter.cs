using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(int))]
    public class AddValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            _ = parameter ?? throw new ArgumentNullException(nameof(parameter));
            var result = value;
            if ((value != null) && int.TryParse((string)parameter, NumberStyles.Integer, culture, out var parameterValue))
            {
                result = (int)value + parameterValue;
            }

            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
