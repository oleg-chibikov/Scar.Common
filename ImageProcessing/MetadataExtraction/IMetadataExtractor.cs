using System;
using System.Threading.Tasks;
using Scar.Common.ImageProcessing.Metadata;

namespace Scar.Common.ImageProcessing.MetadataExtraction;

public interface IMetadataExtractor
{
    Task<ExifMetadata> ExtractAsync(string filePath, MetadataOptions options = MetadataOptions.All);

    Task LoadLocationInfoAsync(ExifMetadata exifMetadata, Action<string?>? onComplete = null);
}
