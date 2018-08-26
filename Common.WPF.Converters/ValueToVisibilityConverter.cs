using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public abstract class ValueToVisibilityConverter<T> : IValueConverter
    {
        [NotNull]
        public object Convert(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            return IsVisible((T)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        [NotNull]
        public object ConvertBack(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        protected abstract bool IsVisible([CanBeNull] T value);
    }
}