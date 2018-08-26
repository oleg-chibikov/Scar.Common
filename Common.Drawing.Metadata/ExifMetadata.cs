using System;
using JetBrains.Annotations;

namespace Scar.Common.Drawing.Metadata
{
    public class ExifMetadata
    {
        public ExifMetadata(
            [CanBeNull] object width,
            [CanBeNull] object height,
            [CanBeNull] string cameraModel,
            [CanBeNull] object lensAperture,
            [CanBeNull] object focalLength,
            [CanBeNull] object isoSpeed,
            [CanBeNull] object exposureTime,
            [CanBeNull] DateTime? dateImageTaken,
            Orientation orientation,
            [CanBeNull] byte[] thumbnailBytes)
        {
            Width = width;
            Height = height;
            CameraModel = cameraModel;
            LensAperture = lensAperture;
            FocalLength = focalLength;
            IsoSpeed = isoSpeed;
            ExposureTime = exposureTime;
            DateImageTaken = dateImageTaken;
            Orientation = orientation;
            ThumbnailBytes = thumbnailBytes;
        }

        public ExifMetadata()
        {
        }

        [CanBeNull]
        public object Width { get; }

        [CanBeNull]
        public object Height { get; }

        [CanBeNull]
        public string CameraModel { get; }

        [CanBeNull]
        public object LensAperture { get; }

        [CanBeNull]
        public object FocalLength { get; }

        [CanBeNull]
        public object IsoSpeed { get; }

        [CanBeNull]
        public object ExposureTime { get; }

        [CanBeNull]
        public DateTime? DateImageTaken { get; set; }

        public Orientation Orientation { get; set; }

        [CanBeNull]
        public byte[] ThumbnailBytes { get; }
    }
}