using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Scar.Common.ImageProcessing.MetadataExtraction;

public class ReverseGeocoder : IReverseGeocoder
{
    public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var apiUrl = new Uri(
            $"https://nominatim.openstreetmap.org/reverse?format=xml&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1&format=json");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36");

        var response = await httpClient.GetAsync(apiUrl).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<OpenStreetMapReverseGeocodingResponse>(json);
            return result?.DisplayName;
        }

        throw new InvalidOperationException("Failed to retrieve location information.");
    }
}
