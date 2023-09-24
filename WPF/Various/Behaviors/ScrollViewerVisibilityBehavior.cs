using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Scar.Common.Events;

namespace Scar.Common.WPF.Behaviors;

public class ScrollViewerVisibilityBehavior : Behavior<ScrollViewer>
{
    public static readonly DependencyProperty NestedListBoxProperty =
        DependencyProperty.Register(nameof(NestedListBox), typeof(ListBox), typeof(ScrollViewerVisibilityBehavior));

    public static readonly DependencyProperty ItemVisibleCommandProperty =
        DependencyProperty.Register(
            nameof(ItemVisibleCommand),
            typeof(ICommand),
            typeof(ScrollViewerVisibilityBehavior),
            new PropertyMetadata(null, OnItemVisibleCommandChanged));

    public static readonly DependencyProperty ScrollViewLoadedCommandProperty =
        DependencyProperty.Register(
            nameof(ScrollViewLoadedCommand),
            typeof(ICommand),
            typeof(ScrollViewerVisibilityBehavior),
            new PropertyMetadata(null, OnScrollViewLoadedCommandChanged));

    public event EventHandler<EventArgs<IList<object>>>? ItemsVisible;

    public event EventHandler<EventArgs<Action<double?>>>? ControlLoaded;

    public ListBox? NestedListBox
    {
        get { return (ListBox)GetValue(NestedListBoxProperty); }
        set { SetValue(NestedListBoxProperty, value); }
    }

    public ICommand ItemVisibleCommand
    {
        get { return (ICommand)GetValue(ItemVisibleCommandProperty); }
        set { SetValue(ItemVisibleCommandProperty, value); }
    }

    public ICommand ScrollViewLoadedCommand
    {
        get { return (ICommand)GetValue(ScrollViewLoadedCommandProperty); }
        set { SetValue(ScrollViewLoadedCommandProperty, value); }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.ScrollChanged += ScrollViewer_ScrollChanged;
        AssociatedObject.Loaded += ScrollViewer_Loaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.ScrollChanged -= ScrollViewer_ScrollChanged;
        AssociatedObject.Loaded -= ScrollViewer_Loaded;
    }

    static ICommand GetItemVisibleCommand(DependencyObject obj) => (ICommand)obj.GetValue(ItemVisibleCommandProperty);

    static ICommand GetScrollViewLoadedCommand(DependencyObject obj) => (ICommand)obj.GetValue(ScrollViewLoadedCommandProperty);

    static void OnItemVisibleCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is ScrollViewerVisibilityBehavior behavior)
        {
            behavior.ItemsVisible += (_, args) =>
            {
                var command = GetItemVisibleCommand(behavior);
                if (command.CanExecute(args))
                {
                    command.Execute(args.Parameter);
                }
            };
        }
    }

    static void OnScrollViewLoadedCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is ScrollViewerVisibilityBehavior behavior)
        {
            behavior.ControlLoaded += (_, args) =>
            {
                var command = GetScrollViewLoadedCommand(behavior);
                if (command.CanExecute(args))
                {
                    command.Execute(args.Parameter);
                }
            };
        }
    }

    void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
    {
        if (NestedListBox == null)
        {
            throw new InvalidOperationException("Nested ListBox is null");
        }

        ControlLoaded?.Invoke(this, new EventArgs<Action<double?>>(Action));
        return;

        void Action(double? offset) => Dispatcher.Invoke(() =>
        {
            ReportVisibleItems();
            if (offset != null)
            {
                var associatedObject = (ScrollViewer)sender;
                associatedObject.ScrollToVerticalOffset(offset.Value);
                associatedObject.UpdateLayout();
            }
        });
    }

    void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.VerticalChange == 0)
        {
            return;
        }

        ReportVisibleItems();
    }

    void ReportVisibleItems()
    {
        if (NestedListBox == null)
        {
            return;
        }

        var visibleItems = new List<object>();

        foreach (var item in NestedListBox.Items)
        {
            if (NestedListBox.ItemContainerGenerator.ContainerFromItem(item) is not ListBoxItem listBoxItem)
            {
                continue;
            }

            var transform = listBoxItem.TransformToAncestor(AssociatedObject);
            var rect = transform.TransformBounds(
                new Rect(
                    0,
                    0,
                    listBoxItem.ActualWidth,
                    listBoxItem.ActualHeight));

            if (rect.IntersectsWith(new Rect(0, 0, AssociatedObject.ActualWidth, AssociatedObject.ActualHeight)))
            {
                visibleItems.Add(item);
            }
        }

        if (visibleItems.Count > 0)
        {
            ItemsVisible?.Invoke(this, new EventArgs<IList<object>>(visibleItems));
        }
    }
}
