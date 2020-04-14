using System.Windows;
using System.Windows.Media;

namespace Scar.Common.WPF.Controls
{
    static class TreeHelper
    {
        public static TChildItem? FindVisualChild<TChildItem>(DependencyObject obj)
            where TChildItem : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (child is TChildItem item)
                {
                    return item;
                }

                var childOfChild = FindVisualChild<TChildItem>(child);

                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }
    }
}
