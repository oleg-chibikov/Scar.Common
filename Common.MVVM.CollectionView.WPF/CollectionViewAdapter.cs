using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

namespace Scar.Common.MVVM.CollectionView.WPF
{
    public class CollectionViewAdapter : ICollectionView
    {
        private readonly System.ComponentModel.ICollectionView _adaptee;

        public CollectionViewAdapter(System.ComponentModel.ICollectionView adaptee)
        {
            _adaptee = adaptee ?? throw new ArgumentNullException(nameof(adaptee));
        }

        public CultureInfo Culture { get => _adaptee.Culture; set => _adaptee.Culture = value; }

        public IEnumerable SourceCollection => _adaptee.SourceCollection;

        public Predicate<object> Filter { get => _adaptee.Filter; set => _adaptee.Filter = value; }

        public bool CanFilter => _adaptee.CanFilter;

        public bool CanSort => _adaptee.CanSort;

        public bool CanGroup => _adaptee.CanGroup;

        public System.Collections.ObjectModel.ReadOnlyObservableCollection<object> Groups => _adaptee.Groups;

        public bool IsEmpty => _adaptee.IsEmpty;

        public object CurrentItem => _adaptee.CurrentItem;

        public int CurrentPosition => _adaptee.CurrentPosition;

        public bool IsCurrentAfterLast => _adaptee.IsCurrentAfterLast;

        public bool IsCurrentBeforeFirst => _adaptee.IsCurrentBeforeFirst;

        public event EventHandler CurrentChanged
        {
            add { _adaptee.CurrentChanged += value; }
            remove { _adaptee.CurrentChanged -= value; }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _adaptee.CollectionChanged += value; }
            remove { _adaptee.CollectionChanged -= value; }
        }

        public bool Contains(object item) => _adaptee.Contains(item);

        public IDisposable DeferRefresh() => _adaptee.DeferRefresh();

        public IEnumerator GetEnumerator() => _adaptee.GetEnumerator();

        public bool MoveCurrentTo(object item) => _adaptee.MoveCurrentTo(item);

        public bool MoveCurrentToFirst() => _adaptee.MoveCurrentToFirst();

        public bool MoveCurrentToLast() => _adaptee.MoveCurrentToLast();

        public bool MoveCurrentToNext() => _adaptee.MoveCurrentToNext();

        public bool MoveCurrentToPosition(int position) => _adaptee.MoveCurrentToPosition(position);

        public bool MoveCurrentToPrevious() => _adaptee.MoveCurrentToPrevious();

        public void Refresh() => _adaptee.Refresh();
    }
}