using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Scar.Common.ImageProcessing.MetadataExtraction;

public class Address
{
    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state_district")]
    public string? StateDistrict { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("ISO3166-2-lvl4")]
    public string? Iso3166_2_lvl4 { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }
}

public class ExtraTags
{
    [JsonPropertyName("capital")]
    public string? Capital { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("wikidata")]
    public string? Wikidata { get; set; }

    [JsonPropertyName("wikipedia")]
    public string? Wikipedia { get; set; }

    [JsonPropertyName("population")]
    public string? Population { get; set; }
}

public class OpenStreetMapReverseGeocodingResponse
{
    [JsonPropertyName("place_id")]
    public int? PlaceId { get; set; }

    [JsonPropertyName("licence")]
    public string? Licence { get; set; }

    [JsonPropertyName("osm_type")]
    public string? OsmType { get; set; }

    [JsonPropertyName("osm_id")]
    public long? OsmId { get; set; }

    [JsonPropertyName("boundingbox")]
    public IReadOnlyCollection<string>? BoundingBox { get; set; }

    [JsonPropertyName("lat")]
    public string? Lat { get; set; }

    [JsonPropertyName("lon")]
    public string? Lon { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("class")]
    public string? Class { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("importance")]
    public double? Importance { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("address")]
    public Address? Address { get; set; }

    [JsonPropertyName("extratags")]
    public ExtraTags? ExtraTags { get; set; }
}
