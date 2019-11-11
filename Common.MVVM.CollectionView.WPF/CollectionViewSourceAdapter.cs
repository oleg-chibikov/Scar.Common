using Scar.Common.MVVM.CollectionView;

namespace Scar.Common.WPF.CollectionView
{
    public class CollectionViewSourceAdapter : ICollectionViewSource
    {
        public ICollectionView GetDefaultView(object source) => new CollectionViewAdapter(System.Windows.Data.CollectionViewSource.GetDefaultView(source));
    }
}