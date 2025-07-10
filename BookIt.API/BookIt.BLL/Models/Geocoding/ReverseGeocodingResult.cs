using System.Text.Json.Serialization;

namespace BookIt.BLL.Models.Geocoding;

public record ReverseGeocodingResult
{
    [JsonPropertyName("lat")]
    public string Latitude { get; set; } = string.Empty;

    [JsonPropertyName("lon")]
    public string Longitude { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public Address Address { get; set; } = null!;

    [JsonPropertyName("display_name")]
    public string DisplayAddress { get; set; } = string.Empty;
}

public record Address
{
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }
}