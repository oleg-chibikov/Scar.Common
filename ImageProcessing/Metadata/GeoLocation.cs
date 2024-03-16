namespace Scar.Common.ImageProcessing.Metadata;

public record GeoLocation
{
    public GeoLocation(double latitude, double longitude, string? name = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        Name = name;
    }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string? Name { get; set; }
}
