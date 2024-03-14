using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Scar.Common.ImageProcessing.Metadata;

namespace Scar.Common.WPF.ImageRetrieval;

public interface IImageRetriever
{
    BitmapSource ApplyRotateTransform(int angle, BitmapSource image);

    Task<byte[]> GetThumbnailAsync(string filePath, CancellationToken cancellationToken);

    Task<BitmapSource?> LoadImageAsync(IReadOnlyCollection<byte>? imageData, CancellationToken cancellationToken, Orientation? orientation = default, int sizeAnchor = 0);
}
