namespace Scar.Common.MVVM.CollectionView
{
    public interface ICollectionViewSource
    {
        ICollectionView GetDefaultView(object source);
    }
}
