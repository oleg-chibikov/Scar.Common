using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Scar.Common.Drawing.Metadata;

namespace Scar.Common.Drawing.ImageRetriever
{
    public interface IImageRetriever
    {
        [NotNull]
        BitmapSource ApplyRotateTransform(int angle, [NotNull] BitmapSource image);

        [NotNull]
        [ItemNotNull]
        Task<byte[]> GetThumbnailAsync([NotNull] string filePath, CancellationToken cancellationToken);

        [NotNull]
        [ItemCanBeNull]
        Task<BitmapSource> LoadImageAsync([CanBeNull] byte[] imageData, CancellationToken cancellationToken, [CanBeNull] Orientation? orientation = default, int sizeAnchor = 0);
    }
}