using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExifLib;
using Microsoft.Extensions.Logging;
using Scar.Common.ImageProcessing.Metadata;

namespace Scar.Common.ImageProcessing.MetadataExtraction
{
    public sealed class MetadataExtractor : IMetadataExtractor
    {
        static readonly string[] JpegExtensions =
        {
            ".jpg",
            ".jpeg"
        };

        static readonly TimeSpan DefaultAttemptDelay = TimeSpan.FromMilliseconds(100);
        readonly ILogger _logger;

        public MetadataExtractor(ILogger<MetadataExtractor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ExifMetadata> ExtractAsync(string filePath)
        {
            _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
            if (!JpegExtensions.Any(x => filePath.EndsWith(x, true, CultureInfo.CurrentCulture)))
            {
                return new ExifMetadata();
            }

            Func<AttemptInfo, ExifMetadata> func = _ =>
            {
                using var reader = new ExifReader(filePath);
                DateTime? dateImageTaken = null;
                var orientation = Orientation.Straight;
                reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime d);
                if (d != default)
                {
                    dateImageTaken = d;
                }

                reader.GetTagValue(ExifTags.PixelXDimension, out object width);
                reader.GetTagValue(ExifTags.PixelYDimension, out object height);
                reader.GetTagValue(ExifTags.Model, out string cameraModel);
                reader.GetTagValue(ExifTags.MaxApertureValue, out object lensAperture);
                reader.GetTagValue(ExifTags.FocalLength, out object focalLength);
                reader.GetTagValue(ExifTags.PhotographicSensitivity, out object isoSpeed);
                reader.GetTagValue(ExifTags.ExposureTime, out object exposureTime);
                reader.GetTagValue(ExifTags.Orientation, out ushort o);
                if (o != default(ushort))
                {
                    orientation = (Orientation)o;
                }

                var thumbnailBytes = reader.GetJpegThumbnailBytes();
                return new ExifMetadata(width, height, cameraModel, lensAperture, focalLength, isoSpeed, exposureTime, dateImageTaken, orientation, thumbnailBytes);
            };

            return await func.RunFuncWithSeveralAttemptsAsync(
                           (attemptInfo, e) =>
                           {
                               if (e is IOException)
                               {
                                   var attemptLog = attemptInfo.HasAttempts ? $"Retrying ({attemptInfo})..." : "No more attempts left";
                                   _logger.LogDebug($"Failed extracting metadata for {filePath} with IO exception. {attemptLog}");
                                   return true;
                               }

                               _logger.LogWarning(e, $"Cannot extract metadata for {filePath}");
                               return false;
                           },
                           DefaultAttemptDelay)
                       .ConfigureAwait(false) ??
                   new ExifMetadata();
        }
    }
}
