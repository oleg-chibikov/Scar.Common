using System.Threading.Tasks;
using Scar.Common.Drawing.Metadata;

namespace Scar.Common.Drawing
{
    public interface IMetadataExtractor
    {
        Task<ExifMetadata> ExtractAsync(string filePath);
    }
}
