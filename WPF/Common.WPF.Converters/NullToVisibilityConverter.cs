using System.Windows;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class NullToVisibilityConverter : ValueToVisibilityConverter<object>
    {
        protected override bool IsVisible(object value)
        {
            return value == null;
        }
    }
}