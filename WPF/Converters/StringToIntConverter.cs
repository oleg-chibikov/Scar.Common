using System;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters;

public class StringToIntConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            if (int.TryParse(stringValue, out var intValue))
            {
                return intValue;
            }
        }

        return Binding.DoNothing;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
