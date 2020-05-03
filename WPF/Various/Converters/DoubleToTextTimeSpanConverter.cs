using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    public enum DateInterval
    {
        Years,
        Months,
        Weeks,
        Days,
        Hours,
        Minutes,
        Seconds,
        Milliseconds
    }

    [ValueConversion(typeof(double), typeof(string))]
    public sealed class DoubleToTextTimeSpanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var val = (double)(value ?? 0);
            var interval = DateInterval.Minutes;
            if (parameter != null)
            {
                interval = (DateInterval)parameter;
            }

            return interval switch
            {
                DateInterval.Years => TimeSpan.FromDays(val * 365).ToReadableFormat(),
                DateInterval.Months => TimeSpan.FromDays(val * 30).ToReadableFormat(),
                DateInterval.Weeks => TimeSpan.FromDays(val * 7).ToReadableFormat(),
                DateInterval.Days => TimeSpan.FromDays(val).ToReadableFormat(),
                DateInterval.Hours => TimeSpan.FromHours(val).ToReadableFormat(),
                DateInterval.Minutes => TimeSpan.FromMinutes(val).ToReadableFormat(),
                DateInterval.Seconds => TimeSpan.FromSeconds(val).ToReadableFormat(),
                DateInterval.Milliseconds => TimeSpan.FromMilliseconds(val).ToReadableFormat(),
                _ => throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null)
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble((int)(value ?? 0));
        }
    }
}
