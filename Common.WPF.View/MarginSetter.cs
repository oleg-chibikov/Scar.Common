using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Scar.Common.WPF.View
{
    public class MarginSetter
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.RegisterAttached(
            "Orientation",
            typeof(Orientation?),
            typeof(MarginSetter),
            new UIPropertyMetadata());

        public static readonly DependencyProperty MarginProperty = DependencyProperty.RegisterAttached(
            "Margin",
            typeof(double),
            typeof(MarginSetter),
            new UIPropertyMetadata(MarginChangedCallback));

        public static double GetMargin(DependencyObject obj)
        {
            return (double)obj.GetValue(MarginProperty);
        }

        public static Orientation? GetOrientation(DependencyObject obj)
        {
            return (Orientation?)obj.GetValue(OrientationProperty);
        }

        public static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is Panel panel))
            {
                return;
            }

            panel.Loaded += Panel_Loaded;
        }

        public static void SetMargin(DependencyObject obj, double value)
        {
            obj.SetValue(MarginProperty, value);
        }

        public static void SetOrientation(DependencyObject obj, Orientation? value)
        {
            obj.SetValue(OrientationProperty, value);
        }

        private static void Panel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is Panel panel))
            {
                return;
            }

            var children = panel.Children.OfType<FrameworkElement>().ToArray();

            for (var i = 0; i < children.Length - 1; i++)
            {
                var child = children[i];
                var margin = GetMargin(panel);
                child.Margin = GetOrientation(panel) == Orientation.Horizontal || panel.LogicalOrientationPublic == Orientation.Horizontal
                    ? new Thickness(child.Margin.Left, child.Margin.Top, margin, child.Margin.Bottom)
                    : new Thickness(child.Margin.Left, child.Margin.Top, child.Margin.Right, margin);
            }
        }
    }
}
