using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using BookIt.DAL.Configuration.Settings;

namespace BookIt.BLL.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleOAuthSettings _googleOAuthSettings;

    public GoogleAuthService(
        HttpClient httpClient,
        IOptions<GoogleOAuthSettings> googleOAuthSettings)
    {
        _httpClient = httpClient;
        _googleOAuthSettings = googleOAuthSettings.Value;

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookIt/1.0");
    }

    public string GetLoginUrl()
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = _googleOAuthSettings.ClientId,
            ["redirect_uri"] = _googleOAuthSettings.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["access_type"] = "offline",
            ["prompt"] = "consent"
        };

        return QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", query);
    }

    public async Task<(string Email, string Name)> GetUserEmailAndNameAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Authorization code is required", nameof(code));

        try
        {
            var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = _googleOAuthSettings.ClientId,
                    ["client_secret"] = _googleOAuthSettings.ClientSecret,
                    ["redirect_uri"] = _googleOAuthSettings.RedirectUri,
                    ["grant_type"] = "authorization_code"
                }));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to exchange code for token: {tokenResponse.StatusCode}");
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            using var tokenDoc = JsonDocument.Parse(tokenJson);

            if (!tokenDoc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
            {
                throw new InvalidOperationException("Access token not found in response");
            }

            var accessToken = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("Access token is empty");
            }

            using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var userResponse = await _httpClient.SendAsync(userRequest);
            if (!userResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to get user info: {userResponse.StatusCode}");
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            using var userDoc = JsonDocument.Parse(userJson);
            var root = userDoc.RootElement;

            if (!root.TryGetProperty("email", out var emailElement))
            {
                throw new InvalidOperationException("Email not found in user info");
            }

            var email = emailElement.GetString();
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Email is empty in user info");
            }

            var name = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : string.Empty;

            return (email, name ?? string.Empty);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid response format from Google", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException("Google OAuth request timed out", ex);
        }
    }
}