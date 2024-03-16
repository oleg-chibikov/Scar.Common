using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;

namespace Scar.Common.WPF.Core;

[SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "No need")]
public class DesignTimeResourceDictionary : ResourceDictionary
{
    readonly ObservableCollection<ResourceDictionary> _noopMergedDictionaries = new NoopObservableCollection<ResourceDictionary>();

    public DesignTimeResourceDictionary()
    {
        var fieldInfo = typeof(ResourceDictionary).GetField("_mergedDictionaries", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo?.SetValue(this, _noopMergedDictionaries);
    }

    sealed class NoopObservableCollection<T> : ObservableCollection<T>
    {
        protected override void InsertItem(int index, T item)
        {
            // Only insert items while in Design Mode (VS is hosting the visualization)
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                base.InsertItem(index, item);
            }
        }
    }
}
