using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Scar.Common.WPF.Controls
{
    public static class KeyboardFocusBehavior
    {
        public static readonly DependencyProperty OnProperty = DependencyProperty.RegisterAttached("On", typeof(FrameworkElement), typeof(KeyboardFocusBehavior), new PropertyMetadata(OnSetCallback));

        public static FrameworkElement? GetOn(UIElement element)
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));

            return (FrameworkElement)element.GetValue(OnProperty);
        }

        public static void SetOn(UIElement element, FrameworkElement value)
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            element.SetValue(OnProperty, value);
        }

        static void OnSetCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;
            var target = GetOn(frameworkElement);

            if (target == null)
            {
                return;
            }

            frameworkElement.Loaded += (s, e) =>
            {
                var parent = VisualTreeHelper.GetParent(frameworkElement);
                while ((parent != null) && !(parent is Window))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                var window = parent as Window;
                if ((window?.ShowActivated == true) || (window == null))
                {
                    Keyboard.Focus(target);
                }
            };
        }
    }
}
