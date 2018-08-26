using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(IEnumerable<string>), typeof(string))]
    public class EnumerableConcatConverter : IValueConverter
    {
        public virtual object Convert(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            return !(value is IEnumerable<string> strings) ? null : string.Join(", ", strings);
        }

        public object ConvertBack(object value, [NotNull] Type targetType, object parameter, [NotNull] CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}