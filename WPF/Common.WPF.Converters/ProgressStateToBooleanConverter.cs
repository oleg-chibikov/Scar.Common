using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Shell;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(TaskbarItemProgressState), typeof(bool))]
    public sealed class ProgressStateToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (value != null) && ((TaskbarItemProgressState)value != TaskbarItemProgressState.Normal);
        }

        public object ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
