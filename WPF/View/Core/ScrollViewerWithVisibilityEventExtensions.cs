using System;
using System.Windows;
using System.Windows.Input;

namespace Scar.Common.WPF.View.Core;

public static class ScrollViewerWithVisibilityEventExtensions
{
    public static readonly DependencyProperty ItemVisibleCommandProperty =
        DependencyProperty.RegisterAttached(
            "ItemVisibleCommand",
            typeof(ICommand),
            typeof(ScrollViewerWithVisibilityEventExtensions),
            new PropertyMetadata(null, OnItemVisibleCommandChanged));

    public static readonly DependencyProperty ScrollViewLoadedCommandProperty =
        DependencyProperty.RegisterAttached(
            "ScrollViewLoadedCommand",
            typeof(ICommand),
            typeof(ScrollViewerWithVisibilityEventExtensions),
            new PropertyMetadata(null, OnScrollViewLoadedCommandChanged));

    public static ICommand GetItemVisibleCommand(DependencyObject obj)
    {
        _ = obj ?? throw new ArgumentNullException(nameof(obj));
        return (ICommand)obj.GetValue(ItemVisibleCommandProperty);
    }

    public static void SetItemVisibleCommand(DependencyObject obj, ICommand value)
    {
        _ = obj ?? throw new ArgumentNullException(nameof(obj));
        obj.SetValue(ItemVisibleCommandProperty, value);
    }

    public static ICommand GetScrollViewLoadedCommand(DependencyObject obj)
    {
        _ = obj ?? throw new ArgumentNullException(nameof(obj));
        return (ICommand)obj.GetValue(ScrollViewLoadedCommandProperty);
    }

    public static void SetScrollViewLoadedCommand(DependencyObject obj, ICommand value)
    {
        _ = obj ?? throw new ArgumentNullException(nameof(obj));
        obj.SetValue(ScrollViewLoadedCommandProperty, value);
    }

    static void OnItemVisibleCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is ScrollViewerWithVisibilityEvent scrollViewerWithVisibilityEvent)
        {
            scrollViewerWithVisibilityEvent.ItemsVisible += (_, args) =>
            {
                var command = GetItemVisibleCommand(scrollViewerWithVisibilityEvent);
                if (command.CanExecute(args))
                {
                    command.Execute(args.Parameter);
                }
            };
        }
    }

    static void OnScrollViewLoadedCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is ScrollViewerWithVisibilityEvent scrollViewerWithVisibilityEvent)
        {
            scrollViewerWithVisibilityEvent.ControlLoaded += (_, args) =>
            {
                var command = GetScrollViewLoadedCommand(scrollViewerWithVisibilityEvent);
                if (command.CanExecute(args))
                {
                    command.Execute(args.Parameter);
                }
            };
        }
    }
}
