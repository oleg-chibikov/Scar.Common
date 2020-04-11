using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public abstract class ValueToVisibilityConverter<T> : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : IsVisible((T)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        protected abstract bool IsVisible(T value);
    }
}
