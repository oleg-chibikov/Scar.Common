using System.Windows;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class ReverseBoolToVisibilityConverter : ValueToVisibilityConverter<bool?>
    {
        protected override bool IsVisible(bool? value)
        {
            return value == false;
        }
    }
}