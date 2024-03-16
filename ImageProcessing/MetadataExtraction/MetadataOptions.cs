using System;

namespace Scar.Common.ImageProcessing.MetadataExtraction;

[Flags]
public enum MetadataOptions
{
    None = 0,
    DateTaken = 1,
    Dimensions = 2,
    CameraInfo = 4,
    Thumbnail = 8,
    Orientation = 16,
    Location = 32,
    All = DateTaken | Dimensions | CameraInfo | Orientation | Location
}
