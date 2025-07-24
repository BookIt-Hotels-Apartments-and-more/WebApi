namespace BookIt.DAL.Configuration.Settings;

public class GeocodingSettings
{
    public const string SectionName = "Geocoding";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
}
