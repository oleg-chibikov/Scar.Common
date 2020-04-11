using System.Collections;
using System.Windows;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters
{
    [ValueConversion(typeof(ICollection), typeof(Visibility))]
    public sealed class CollectionToVisibilityConverter : ValueToVisibilityConverter<ICollection>
    {
        protected override bool IsVisible(ICollection value)
        {
            return value == null || value.Count > 0;
        }
    }
}
