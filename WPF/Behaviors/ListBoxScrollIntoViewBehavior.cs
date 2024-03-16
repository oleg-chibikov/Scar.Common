using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Scar.Common.WPF.Behaviors;

public sealed class ListBoxScrollIntoViewBehavior : Behavior<ListBox>
{
    /// <summary>
    /// When Behavior is attached.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    /// <summary>
    /// When behavior is detached.
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
    }

    /// <summary>
    /// On Selection Changed.
    /// </summary>
    static void AssociatedObject_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox)
        {
            return;
        }

        if (listBox.SelectedItems.Count > 1)
        {
            return;
        }

        if (listBox.SelectedItem == null)
        {
            return;
        }

        listBox.Dispatcher.BeginInvoke(
            () =>
            {
                listBox.UpdateLayout();
                if (listBox.SelectedItem != null)
                {
                    listBox.ScrollIntoView(listBox.SelectedItem);
                }
            });
    }
}
