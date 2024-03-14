using System;
using System.Windows;
using System.Windows.Input;

namespace Scar.Common.WPF.Core;

public static class FocusExtensions
{
    public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(
        "IsFocused",
        typeof(bool),
        typeof(FocusExtensions),
        new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

    public static bool GetIsFocused(DependencyObject obj)
    {
        _ = obj ?? throw new ArgumentNullException(nameof(obj));

        return (bool)obj.GetValue(IsFocusedProperty);
    }

    public static void SetIsFocused(DependencyObject obj, bool value)
    {
        _ = obj ?? throw new ArgumentNullException(nameof(obj));

        obj.SetValue(IsFocusedProperty, value);
    }

    static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var target = (UIElement)d;
        if ((bool)e.NewValue)
        {
            target.Focus(); // Don't care about false values
            Keyboard.Focus(target);
        }
    }
}