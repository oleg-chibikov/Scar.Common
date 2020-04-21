using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scar.Common.WPF.Controls.Behaviors
{
    public sealed class ListBoxAutoScrollBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged += OnCollectionChanged;
        }

        protected override void OnDetaching()
        {
            ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged -= OnCollectionChanged;
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                return;
            }

            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            var count = AssociatedObject.Items.Count;
            if (count == 0)
            {
                return;
            }

            var item = AssociatedObject.Items[count - 1];
            AssociatedObject.ScrollIntoView(item);
        }
    }
}
