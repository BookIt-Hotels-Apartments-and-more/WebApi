namespace BookIt.DAL.Configuration.Settings;

public class GoogleOAuthSettings
{
    public const string SectionName = "GoogleOAuth";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}