using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Scar.Common.MVVM.CollectionView;

namespace Scar.Common.WPF.CollectionView
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Name is OK")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "This collection is OK without generic interface")]
    public class CollectionViewAdapter : ICollectionView
    {
        readonly System.ComponentModel.ICollectionView _adaptee;

        public CollectionViewAdapter(System.ComponentModel.ICollectionView adaptee)
        {
            _adaptee = adaptee ?? throw new ArgumentNullException(nameof(adaptee));
        }

        public event EventHandler CurrentChanged
        {
            add => _adaptee.CurrentChanged += value;
            remove => _adaptee.CurrentChanged -= value;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _adaptee.CollectionChanged += value;
            remove => _adaptee.CollectionChanged -= value;
        }

        public CultureInfo Culture { get => _adaptee.Culture; set => _adaptee.Culture = value; }

        public IEnumerable SourceCollection => _adaptee.SourceCollection;

        public Predicate<object>? Filter { get => _adaptee.Filter; set => _adaptee.Filter = value; }

        public bool CanFilter => _adaptee.CanFilter;

        public bool CanSort => _adaptee.CanSort;

        public bool CanGroup => _adaptee.CanGroup;

        public ReadOnlyObservableCollection<object> Groups => _adaptee.Groups;

        public bool IsEmpty => _adaptee.IsEmpty;

        public object CurrentItem => _adaptee.CurrentItem;

        public int CurrentPosition => _adaptee.CurrentPosition;

        public bool IsCurrentAfterLast => _adaptee.IsCurrentAfterLast;

        public bool IsCurrentBeforeFirst => _adaptee.IsCurrentBeforeFirst;

        public bool Contains(object item)
        {
            return _adaptee.Contains(item);
        }

        public IDisposable DeferRefresh()
        {
            return _adaptee.DeferRefresh();
        }

        public IEnumerator GetEnumerator()
        {
            return _adaptee.GetEnumerator();
        }

        public bool MoveCurrentTo(object item)
        {
            return _adaptee.MoveCurrentTo(item);
        }

        public bool MoveCurrentToFirst()
        {
            return _adaptee.MoveCurrentToFirst();
        }

        public bool MoveCurrentToLast()
        {
            return _adaptee.MoveCurrentToLast();
        }

        public bool MoveCurrentToNext()
        {
            return _adaptee.MoveCurrentToNext();
        }

        public bool MoveCurrentToPosition(int position)
        {
            return _adaptee.MoveCurrentToPosition(position);
        }

        public bool MoveCurrentToPrevious()
        {
            return _adaptee.MoveCurrentToPrevious();
        }

        public void Refresh()
        {
            _adaptee.Refresh();
        }
    }
}
