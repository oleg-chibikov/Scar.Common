using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExifLib;
using Microsoft.Extensions.Logging;
using Scar.Common.ImageProcessing.Metadata;

namespace Scar.Common.ImageProcessing.MetadataExtraction;

public sealed class MetadataExtractor(ILogger<MetadataExtractor>? logger = null) : IMetadataExtractor
{
    public static readonly HashSet<string> ImageExtensionsWithMetadata = new() { ".jpg", ".jpeg", };
    public static readonly HashSet<string> ImageExtensions = new()
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".bmp",
        ".tif",
        ".tiff",
        ".webp",
        ".svg",
        ".ico",
        ".exif",
        ".raw"
    };

    static readonly TimeSpan DefaultAttemptDelay = TimeSpan.FromMilliseconds(100);
    readonly ILogger? _logger = logger;

    public async Task<ExifMetadata> ExtractAsync(string filePath, MetadataOptions options = MetadataOptions.All)
    {
        _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
        if (!ImageExtensionsWithMetadata.Any(
                x => filePath.EndsWith(
                    x,
                    ignoreCase: true,
                    CultureInfo.CurrentCulture)))
        {
            return new ExifMetadata();
        }

        Func<AttemptInfo, ExifMetadata> func = _ =>
        {
            using var reader = new ExifReader(filePath);
            DateTime? dateImageTaken = null;
            var orientation = Orientation.Straight;

            if (options.HasFlag(MetadataOptions.DateTaken))
            {
                reader.GetTagValue(
                    ExifTags.DateTimeOriginal,
                    out DateTime d);
                if (d != default)
                {
                    dateImageTaken = d;
                }
            }

            object? width = null,
                height = null,
                cameraModel = null,
                lensAperture = null,
                focalLength = null,
                isoSpeed = null,
                exposureTime = null,
                thumbnailBytes = null;

            if (options.HasFlag(MetadataOptions.Dimensions))
            {
                reader.GetTagValue(
                    ExifTags.PixelXDimension,
                    out width);
                reader.GetTagValue(
                    ExifTags.PixelYDimension,
                    out height);
            }

            if (options.HasFlag(MetadataOptions.CameraInfo))
            {
                reader.GetTagValue(
                    ExifTags.Model,
                    out cameraModel);
                reader.GetTagValue(
                    ExifTags.MaxApertureValue,
                    out lensAperture);
                reader.GetTagValue(
                    ExifTags.FocalLength,
                    out focalLength);
                reader.GetTagValue(
                    ExifTags.PhotographicSensitivity,
                    out isoSpeed);
                reader.GetTagValue(
                    ExifTags.ExposureTime,
                    out exposureTime);
            }

            if (options.HasFlag(MetadataOptions.Thumbnail))
            {
                thumbnailBytes = reader.GetJpegThumbnailBytes();
            }

            if (options.HasFlag(MetadataOptions.Orientation))
            {
                reader.GetTagValue(
                    ExifTags.Orientation,
                    out ushort o);
                if (o != default(ushort))
                {
                    orientation = (Orientation)o;
                }
            }

            return new ExifMetadata(
                width,
                height,
                cameraModel?.ToString(),
                lensAperture,
                focalLength,
                isoSpeed,
                exposureTime,
                dateImageTaken,
                orientation,
                thumbnailBytes as byte[]);
        };

        return await func.RunFuncWithSeveralAttemptsAsync(
                   (attemptInfo, ex) =>
                   {
                       if (ex is IOException)
                       {
                           var attemptLog = attemptInfo.HasAttempts
                               ? $"Retrying ({attemptInfo})..."
                               : "No more attempts left";
                           _logger?.LogDebug(
                               "Failed extracting metadata for {FilePath} with IO exception. {AttemptLog}",
                               filePath,
                               attemptLog);
                           return true;
                       }

                       _logger?.LogWarning(
                           ex,
                           "Cannot extract metadata for {FilePath}",
                           filePath);
                       return false;
                   },
                   DefaultAttemptDelay).ConfigureAwait(false) ??
               new ExifMetadata();
    }
}
