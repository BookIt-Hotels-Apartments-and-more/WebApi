using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Configuration.Settings;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BookIt.BLL.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly GoogleOAuthSettings _googleOAuthSettings;

    public GoogleAuthService(
        HttpClient httpClient,
        ILogger<GoogleAuthService> logger,
        IOptions<GoogleOAuthSettings> googleOAuthSettings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _googleOAuthSettings = googleOAuthSettings?.Value ?? throw new ArgumentNullException(nameof(googleOAuthSettings));

        ValidateGoogleOAuthConfiguration();
        ConfigureHttpClient();
    }

    public string GetLoginUrl()
    {
        try
        {
            _logger.LogInformation("Generating Google OAuth login URL");

            var query = new Dictionary<string, string?>
            {
                ["client_id"] = _googleOAuthSettings.ClientId,
                ["redirect_uri"] = _googleOAuthSettings.RedirectUri,
                ["response_type"] = "code",
                ["scope"] = "openid email profile",
                ["access_type"] = "offline",
                ["prompt"] = "consent"
            };

            var loginUrl = QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", query);

            _logger.LogInformation("Generated Google OAuth login URL successfully");
            return loginUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Google OAuth login URL");
            throw new ExternalServiceException("Google Auth", "Failed to generate login URL", ex);
        }
    }

    public async Task<(string Email, string Name)> GetUserEmailAndNameAsync(string code)
    {
        try
        {
            ValidateAuthorizationCode(code);

            _logger.LogInformation("Starting Google OAuth user info retrieval process");

            var accessToken = await ExchangeCodeForAccessTokenAsync(code);

            var (email, name) = await GetUserInfoAsync(accessToken);

            _logger.LogInformation("Successfully retrieved user info from Google for email: {Email}", email);

            return (email, name);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user email and name from Google");
            throw new ExternalServiceException("Google Auth", "Failed to retrieve user information from Google", ex);
        }
    }

    private void ValidateGoogleOAuthConfiguration()
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(_googleOAuthSettings.ClientId))
            validationErrors.Add("ClientId", new List<string> { "Google OAuth Client ID is required" });

        if (string.IsNullOrWhiteSpace(_googleOAuthSettings.ClientSecret))
            validationErrors.Add("ClientSecret", new List<string> { "Google OAuth Client Secret is required" });

        if (string.IsNullOrWhiteSpace(_googleOAuthSettings.RedirectUri))
            validationErrors.Add("RedirectUri", new List<string> { "Google OAuth Redirect URI is required" });

        if (!string.IsNullOrWhiteSpace(_googleOAuthSettings.RedirectUri) &&
            !Uri.IsWellFormedUriString(_googleOAuthSettings.RedirectUri, UriKind.Absolute))
        {
            validationErrors.Add("RedirectUri", new List<string> { "Google OAuth Redirect URI must be a valid absolute URL" });
        }

        if (validationErrors.Any())
            throw new Exception("Invalid Geoogle OAuth configuration");
    }

    private void ValidateAuthorizationCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Code", "Authorization code is required");
    }

    private void ConfigureHttpClient()
    {
        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookIt/1.0");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Google Auth", "Failed to configure HTTP client", ex);
        }
    }

    private async Task<string> ExchangeCodeForAccessTokenAsync(string code)
    {
        try
        {
            _logger.LogInformation("Exchanging authorization code for access token");

            var tokenRequestData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _googleOAuthSettings.ClientId,
                ["client_secret"] = _googleOAuthSettings.ClientSecret,
                ["redirect_uri"] = _googleOAuthSettings.RedirectUri,
                ["grant_type"] = "authorization_code"
            });

            var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequestData);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                _logger.LogError("Token exchange failed with status {StatusCode}: {Error}",
                    tokenResponse.StatusCode, errorContent);

                throw tokenResponse.StatusCode switch
                {
                    HttpStatusCode.BadRequest => new ValidationException("Code", "Invalid or expired authorization code"),
                    HttpStatusCode.Unauthorized => new ExternalServiceException("Google Auth", "Invalid client credentials", null!, 401),
                    HttpStatusCode.Forbidden => new ExternalServiceException("Google Auth", "Access forbidden - check OAuth configuration", null!, 403),
                    _ => new ExternalServiceException("Google Auth", $"Token exchange failed: {tokenResponse.StatusCode}", null!, (int)tokenResponse.StatusCode)
                };
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            using var tokenDoc = JsonDocument.Parse(tokenJson);

            if (!tokenDoc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
            {
                throw new ExternalServiceException("Google Auth", "Access token not found in Google response");
            }

            var accessToken = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ExternalServiceException("Google Auth", "Access token is empty in Google response");
            }

            _logger.LogInformation("Successfully exchanged code for access token");
            return accessToken;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse token response from Google");
            throw new ExternalServiceException("Google Auth", "Invalid JSON response from Google token endpoint", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during token exchange");
            throw new ExternalServiceException("Google Auth", "Network error during token exchange", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Token exchange request timed out");
            throw new ExternalServiceException("Google Auth", "Token exchange request timed out", ex);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token exchange");
            throw new ExternalServiceException("Google Auth", "Unexpected error during token exchange", ex);
        }
    }

    private async Task<(string Email, string Name)> GetUserInfoAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Retrieving user info from Google");

            using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var userResponse = await _httpClient.SendAsync(userRequest);

            if (!userResponse.IsSuccessStatusCode)
            {
                var errorContent = await userResponse.Content.ReadAsStringAsync();
                _logger.LogError("User info request failed with status {StatusCode}: {Error}",
                    userResponse.StatusCode, errorContent);

                throw userResponse.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new ExternalServiceException("Google Auth", "Invalid or expired access token", null!, 401),
                    HttpStatusCode.Forbidden => new ExternalServiceException("Google Auth", "Insufficient permissions to access user info", null!, 403),
                    HttpStatusCode.TooManyRequests => new ExternalServiceException("Google Auth", "Rate limit exceeded", null!, 429),
                    _ => new ExternalServiceException("Google Auth", $"Failed to get user info: {userResponse.StatusCode}", null!, (int)userResponse.StatusCode)
                };
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            using var userDoc = JsonDocument.Parse(userJson);
            var root = userDoc.RootElement;

            if (!root.TryGetProperty("email", out var emailElement))
                throw new ExternalServiceException("Google Auth", "Email not found in user info response");

            var email = emailElement.GetString();
            if (string.IsNullOrWhiteSpace(email))
                throw new ExternalServiceException("Google Auth", "Email is empty in user info response");

            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ExternalServiceException("Google Auth", "Invalid email format received from Google");

            var name = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : string.Empty;

            if (!string.IsNullOrWhiteSpace(name) && name.Length > 100)
            {
                _logger.LogWarning("Received unusually long name from Google: {NameLength} characters", name.Length);
                name = name.Substring(0, 100);
            }

            _logger.LogInformation("Successfully retrieved user info from Google");
            return (email, name ?? string.Empty);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse user info response from Google");
            throw new ExternalServiceException("Google Auth", "Invalid JSON response from Google user info endpoint", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during user info retrieval");
            throw new ExternalServiceException("Google Auth", "Network error during user info retrieval", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "User info request timed out");
            throw new ExternalServiceException("Google Auth", "User info request timed out", ex);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user info retrieval");
            throw new ExternalServiceException("Google Auth", "Unexpected error during user info retrieval", ex);
        }
    }
}