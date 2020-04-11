using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Scar.Common.Drawing.Metadata;

namespace Scar.Common.Drawing.ImageRetriever
{
    public interface IImageRetriever
    {
        BitmapSource ApplyRotateTransform(int angle, BitmapSource image);

        Task<byte[]> GetThumbnailAsync(string filePath, CancellationToken cancellationToken);

        Task<BitmapSource?> LoadImageAsync(byte[]? imageData, CancellationToken cancellationToken, Orientation? orientation = default, int sizeAnchor = 0);
    }
}
