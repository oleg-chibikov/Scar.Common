using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(string), typeof(ImageSource))]
    public sealed class FilePathToIconImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(value is string filePath) ? null : File.Exists(filePath) ? Icon.ExtractAssociatedIcon(filePath)?.ToImageSource() : null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
