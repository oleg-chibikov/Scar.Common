using System.Threading.Tasks;
using Scar.Common.Drawing.Metadata;

namespace Scar.Common.ImageProcessing.MetadataExtractor
{
    public interface IMetadataExtractor
    {
        Task<ExifMetadata> ExtractAsync(string filePath);
    }
}
