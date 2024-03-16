using System.Threading.Tasks;

namespace Scar.Common.ImageProcessing.MetadataExtraction;

public interface IReverseGeocoder
{
    Task<string?> ReverseGeocodeAsync(double latitude, double longitude);
}
