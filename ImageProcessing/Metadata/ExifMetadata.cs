using System;
using System.Collections.Generic;

namespace Scar.Common.ImageProcessing.Metadata;

public class ExifMetadata
{
    public ExifMetadata(
        object? width,
        object? height,
        string? cameraModel,
        object? lensAperture,
        object? focalLength,
        object? isoSpeed,
        object? exposureTime,
        DateTime? dateImageTaken,
        Orientation orientation,
        IReadOnlyCollection<byte>? thumbnailBytes,
        GeoLocation? geoLocation) : this(dateImageTaken)
    {
        Width = width;
        Height = height;
        CameraModel = cameraModel;
        LensAperture = lensAperture;
        FocalLength = focalLength;
        IsoSpeed = isoSpeed;
        ExposureTime = exposureTime;
        Orientation = orientation;
        ThumbnailBytes = thumbnailBytes;
        GeoLocation = geoLocation;
    }

    public ExifMetadata(DateTime? dateImageTaken)
    {
        DateImageTaken = dateImageTaken;
    }

    public ExifMetadata()
    {
    }

    public object? Width { get; }

    public object? Height { get; }

    public string? CameraModel { get; }

    public object? LensAperture { get; }

    public object? FocalLength { get; }

    public object? IsoSpeed { get; }

    public object? ExposureTime { get; }

    public GeoLocation? GeoLocation { get; set; }

    public DateTime? DateImageTaken { get; set; }

    public Orientation Orientation { get; set; }

    public IReadOnlyCollection<byte>? ThumbnailBytes { get; }
}
