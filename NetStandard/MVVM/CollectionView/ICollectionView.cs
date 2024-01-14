using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Scar.Common.MVVM.CollectionView;

/// <summary>
/// ICollectionView is an interface that applications writing their own
/// collections can implement to enable current record management, sorting,
/// filtering, grouping etc in a custom way.
/// </summary>
[SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Implementation is OK")]
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Name is OK")]
public interface ICollectionView : IEnumerable, INotifyCollectionChanged
{
    /// <summary>
    /// Raise this event after changing to a new current item.
    /// </summary>
    event EventHandler CurrentChanged;

    /// <summary>
    /// Gets or sets culture that contains the CultureInfo used in any operations of the
    /// ICollectionView that may differ by Culture, such as sorting.
    /// </summary>
    CultureInfo Culture { get; set; }

    /// <summary>
    /// Gets sourceCollection that is the original un-filtered collection of which
    /// this ICollectionView is a view.
    /// </summary>
    IEnumerable SourceCollection { get; }

    /// <summary>
    /// Gets or sets filter which is a callback set by the consumer of the ICollectionView
    /// and used by the implementation of the ICollectionView to determine if an
    /// item is suitable for inclusion in the view.
    /// </summary>
    Predicate<object>? Filter { get; set; }

    /// <summary>
    /// Gets a value indicating whether this ICollectionView can do any filtering.
    /// </summary>
    bool CanFilter { get; }

    /// <summary>
    /// Gets a value indicating whether this ICollectionView does any sorting.
    /// </summary>
    bool CanSort { get; }

    /// <summary>
    /// Gets a value indicating whether this view really supports grouping.
    /// When this returns false, the rest of the interface is ignored.
    /// </summary>
    bool CanGroup { get; }

    /// <summary>
    /// Gets the top-level groups, constructed according to the descriptions
    /// given in GroupDescriptions.
    /// </summary>
    ReadOnlyObservableCollection<object> Groups { get; }

    /// <summary>
    /// Gets a value indicating whether the resulting (filtered) view is empty.
    /// </summary>
    bool IsEmpty { get; }

    // CurrentItem

    /// <summary>
    /// Gets current item.
    /// </summary>
    object CurrentItem { get; }

    /// <summary>
    /// Gets the ordinal position of the <seealso cref="CurrentItem" /> within the (optionally
    /// sorted and filtered) view.
    /// </summary>
    int CurrentPosition { get; }

    /// <summary>
    /// Gets a value indicating whether <seealso cref="CurrentItem" /> is beyond the end (End-Of-File).
    /// </summary>
    bool IsCurrentAfterLast { get; }

    /// <summary>
    /// Gets a value indicating whether <seealso cref="CurrentItem" /> is before the beginning (Beginning-Of-File).
    /// </summary>
    bool IsCurrentBeforeFirst { get; }

    /// <summary>
    /// Return true if the item belongs to this view.  No assumptions are
    /// made about the item. This method will behave similarly to IList.Contains().
    /// If the caller knows that the item belongs to the
    /// underlying collection, it is more efficient to call <seealso cref="Filter" />.
    /// </summary>
    bool Contains(object item);

    /// <summary>
    /// Re-create the view, using any SortDescriptions.
    /// </summary>
    void Refresh();

    /// <summary>
    /// Enter a Defer Cycle.
    /// Defer cycles are used to coalesce changes to the ICollectionView.
    /// </summary>
    IDisposable DeferRefresh();

    /// <summary>
    /// Move <seealso cref="CurrentItem" /> to the first item.
    /// </summary>
    /// <returns>true if <seealso cref="CurrentItem" /> points to an item within the view.</returns>
    bool MoveCurrentToFirst();

    /// <summary>
    /// Move <seealso cref="CurrentItem" /> to the last item.
    /// </summary>
    /// <returns>true if <seealso cref="CurrentItem" /> points to an item within the view.</returns>
    bool MoveCurrentToLast();

    /// <summary>
    /// Move <seealso cref="CurrentItem" /> to the next item.
    /// </summary>
    /// <returns>true if <seealso cref="CurrentItem" /> points to an item within the view.</returns>
    bool MoveCurrentToNext();

    /// <summary>
    /// Move <seealso cref="CurrentItem" /> to the previous item.
    /// </summary>
    /// <returns>true if <seealso cref="CurrentItem" /> points to an item within the view.</returns>
    bool MoveCurrentToPrevious();

    /// <summary>
    /// Move <seealso cref="CurrentItem" /> to the given item.
    /// </summary>
    /// <param name="item">Move CurrentItem to this item.</param>
    /// <returns>true if <seealso cref="CurrentItem" /> points to an item within the view.</returns>
    bool MoveCurrentTo(object item);

    /// <summary>
    /// Move <seealso cref="CurrentItem" /> to the item at the given index.
    /// </summary>
    /// <param name="position">Move CurrentItem to this index.</param>
    /// <returns>true if <seealso cref="CurrentItem" /> points to an item within the view.</returns>
    bool MoveCurrentToPosition(int position);
}