using System.ComponentModel.DataAnnotations;

namespace BookIt.BLL.Models.Geocoding;

public record ReverseGeocodingOptions
{
    public double Lat { get; set; }

    public double Lon { get; set; }

    [Range(0, 18)]
    public int Zoom { get; set; } = 18;

    [AllowedValues(0, 1)]
    public int AddressDetails { get; set; } = 1;

    [AllowedValues(0, 1)]
    public int NameDetails { get; set; } = 1;

    [AllowedValues(0, 1)]
    public int ExtraTags { get; set; } = 1;
}
