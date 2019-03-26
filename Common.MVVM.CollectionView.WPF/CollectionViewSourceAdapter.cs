namespace Scar.Common.MVVM.CollectionView.WPF
{
    public class CollectionViewSourceAdapter : ICollectionViewSource
    {
        public ICollectionView GetDefaultView(object source) => new CollectionViewAdapter(System.Windows.Data.CollectionViewSource.GetDefaultView(source));
    }
}