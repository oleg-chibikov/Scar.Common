using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters;

[ValueConversion(typeof(IEnumerable<string>), typeof(string))]
public class EnumerableConcatConverter : IValueConverter
{
    public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not IEnumerable<string> strings ? null : string.Join(", ", strings);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}