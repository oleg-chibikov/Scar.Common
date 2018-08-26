using System.Windows;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public sealed class StringToVisibilityConverter : ValueToVisibilityConverter<string>
    {
        protected override bool IsVisible(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}