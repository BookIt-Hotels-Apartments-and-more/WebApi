using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace BookIt.BLL.Services;

public class GoogleOAuthSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
}

public class GoogleAuthService
{
    private readonly GoogleOAuthSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleAuthService(IOptions<GoogleOAuthSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
    }

    public string GetLoginUrl()
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = "932606301370-0cip35440rogmahvu5o3lpcp64q3h7fn.apps.googleusercontent.com",
            ["redirect_uri"] = "http://localhost:5173/google-auth/callback",
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["access_type"] = "offline",
            ["prompt"] = "consent"
        };

        return QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", query);
    }

    public async Task<(string Email, string Name)> GetUserEmailAndNameAsync(string code)
    {
        var client = _httpClientFactory.CreateClient();

        var tokenResponse = await client.PostAsync("https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = "932606301370-0cip35440rogmahvu5o3lpcp64q3h7fn.apps.googleusercontent.com",
                ["client_secret"] = "",
                ["redirect_uri"] = "http://localhost:5173/google-auth/callback",
                ["grant_type"] = "authorization_code"
            }));

        if (!tokenResponse.IsSuccessStatusCode)
            throw new Exception("Failed to get token");

        var tokenData = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        var accessToken = tokenData.RootElement.GetProperty("access_token").GetString();

        var req = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var userResponse = await client.SendAsync(req);
        if (!userResponse.IsSuccessStatusCode)
            throw new Exception("Failed to get user info");

        var userInfo = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync());
        var email = userInfo.RootElement.GetProperty("email").GetString();
        var name = userInfo.RootElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "";

        return (email, name);
    }
}
