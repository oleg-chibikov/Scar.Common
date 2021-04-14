using System;
using System.Windows;
using System.Windows.Input;

namespace Scar.Common.WPF.View.Core
{
    public static class ControlExtensions
    {
        public static void PreventFocusLoss(this UIElement uiElement)
        {
            _ = uiElement ?? throw new ArgumentNullException(nameof(uiElement));

            uiElement.PreviewLostKeyboardFocus += Control_PreviewLostKeyboardFocus;
        }

        static void Control_PreviewLostKeyboardFocus(object? sender, KeyboardFocusChangedEventArgs e)
        {
            if ((e.NewFocus != null) && (!e.NewFocus.Focusable || !e.NewFocus.IsEnabled))
            {
                e.Handled = true;
            }
        }
    }
}
