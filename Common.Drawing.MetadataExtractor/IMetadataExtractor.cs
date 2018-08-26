using System.Threading.Tasks;
using JetBrains.Annotations;
using Scar.Common.Drawing.Metadata;

namespace Scar.Common.Drawing.MetadataExtractor
{
    public interface IMetadataExtractor
    {
        [NotNull]
        [ItemNotNull]
        Task<ExifMetadata> ExtractAsync([NotNull] string filePath);
    }
}