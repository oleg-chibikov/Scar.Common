using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Scar.Common.Events;

namespace Scar.Common.WPF.View.Core
{
    public class ScrollViewerWithVisibilityEvent : ScrollViewer
    {
        public static readonly DependencyProperty NestedListBoxProperty =
            DependencyProperty.Register("NestedListBox", typeof(ListBox), typeof(ScrollViewerWithVisibilityEvent));

        public ScrollViewerWithVisibilityEvent()
        {
            ScrollChanged += ScrollViewer_ScrollChanged;
            Loaded += ScrollViewerWithVisibilityEvent_Loaded;
        }

        public event EventHandler<EventArgs<IList<object>>>? ItemsVisible;

        public event EventHandler<EventArgs<Action>>? ControlLoaded;

        public ListBox? NestedListBox
        {
            get { return (ListBox)GetValue(NestedListBoxProperty); }
            set { SetValue(NestedListBoxProperty, value); }
        }

        void ScrollViewerWithVisibilityEvent_Loaded(object sender, RoutedEventArgs e)
        {
            if (NestedListBox == null)
            {
                throw new InvalidOperationException("Nested ListBox is null");
            }

            ControlLoaded?.Invoke(this, new EventArgs<Action>(Action));
            return;

            void Action() => Dispatcher.Invoke(ReportVisibleItems);
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

            // Check if items are currently in the viewport
            foreach (var item in NestedListBox.Items)
            {
                if (NestedListBox.ItemContainerGenerator.ContainerFromItem(item) is not ListBoxItem listBoxItem)
                {
                    continue;
                }

                var transform = listBoxItem.TransformToAncestor(this);
                var rect = transform.TransformBounds(
                    new Rect(
                        0,
                        0,
                        listBoxItem.ActualWidth,
                        listBoxItem.ActualHeight));

                if (rect.IntersectsWith(new Rect(0, 0, ActualWidth, ActualHeight)))
                {
                    visibleItems.Add(item);
                }
            }

            if (visibleItems.Count > 0)
            {
                // Fire the ItemVisible event for items in the viewport
                ItemsVisible?.Invoke(this, new EventArgs<IList<object>>(visibleItems));
            }
        }
    }
}
