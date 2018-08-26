using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using ExifLib;
using JetBrains.Annotations;
using Scar.Common.Drawing.Metadata;

namespace Scar.Common.Drawing.MetadataExtractor
{
    public sealed class MetadataExtractor : IMetadataExtractor
    {
        [NotNull]
        private static readonly string[] JpegExtensions =
        {
            ".jpg",
            ".jpeg"
        };

        private static readonly TimeSpan DefaultAttemptDelay = TimeSpan.FromMilliseconds(100);

        [NotNull]
        private readonly ILog _logger;

        public MetadataExtractor([NotNull] ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ExifMetadata> ExtractAsync(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!JpegExtensions.Any(x => filePath.EndsWith(x, true, CultureInfo.CurrentCulture)))
            {
                return new ExifMetadata();
            }

            Func<AttemptInfo, ExifMetadata> func = attemptInfo =>
            {
                using (var reader = new ExifReader(filePath))
                {
                    DateTime? dateImageTaken = null;
                    var orientation = Orientation.Straight;
                    reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime d);
                    if (d != default(DateTime))
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
                }
            };

            return await func.RunFuncWithSeveralAttemptsAsync(
                           (attemptInfo, e) =>
                           {
                               if (e is IOException)
                               {
                                   var attemptLog = attemptInfo.HasAttempts ? $"Retrying ({attemptInfo})..." : "No more attempts left";
                                   _logger.Debug($"Failed extracting metadata for {filePath} with IO exception. {attemptLog}");
                                   return true;
                               }

                               _logger.Warn($"Cannot extract metadata for {filePath}", e);
                               return false;
                           },
                           DefaultAttemptDelay)
                       .ConfigureAwait(false)
                   ?? new ExifMetadata();
        }
    }
}