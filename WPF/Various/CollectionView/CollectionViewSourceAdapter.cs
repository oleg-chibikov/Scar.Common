using System.Windows.Data;
using Scar.Common.MVVM.CollectionView;

namespace Scar.Common.WPF.CollectionView
{
    public class CollectionViewSourceAdapter : ICollectionViewSource
    {
        public ICollectionView GetDefaultView(object source)
        {
            return new CollectionViewAdapter(CollectionViewSource.GetDefaultView(source));
        }
    }
}
