using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

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
        [NotNull]
        public object Convert(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            var val = (double)(value ?? 0);
            var interval = DateInterval.Minutes;
            if (parameter != null)
            {
                interval = (DateInterval)parameter;
            }

            switch (interval)
            {
                case DateInterval.Years:
                    return TimeSpan.FromDays(val * 365).ToReadableFormat();
                case DateInterval.Months:
                    return TimeSpan.FromDays(val * 30).ToReadableFormat();
                case DateInterval.Weeks:
                    return TimeSpan.FromDays(val * 7).ToReadableFormat();
                case DateInterval.Days:
                    return TimeSpan.FromDays(val).ToReadableFormat();
                case DateInterval.Hours:
                    return TimeSpan.FromHours(val).ToReadableFormat();
                case DateInterval.Minutes:
                    return TimeSpan.FromMinutes(val).ToReadableFormat();
                case DateInterval.Seconds:
                    return TimeSpan.FromSeconds(val).ToReadableFormat();
                case DateInterval.Milliseconds:
                    return TimeSpan.FromMilliseconds(val).ToReadableFormat();
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }

        [NotNull]
        public object ConvertBack(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            return System.Convert.ToDouble((int)(value ?? 0));
        }
    }
}