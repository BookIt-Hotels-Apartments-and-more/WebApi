using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.BLL.Services;
using BookIt.DAL.Configuration.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace BookIt.BLL.Extensions;

public static class CustomHttpClientsRegistrationExtension
{
    public static IServiceCollection AddCustomHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IMonobankAcquiringService, MonobankAcquiringService>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MonobankSettings>>().Value;
            ValidateMonobankConfiguration(settings);
            ConfigureHttpClientForMonobankService(client, settings!);
        });

        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<GoogleOAuthSettings>>().Value;
            ValidateGoogleOAuthConfiguration(settings);
            ConfigureHttpClientForGoogleAuth(client, settings!);
        });

        services.AddHttpClient<IGeolocationService, GeolocationService>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<GeocodingSettings>>().Value;
            ValidateGeocodingConfiguration(settings);
            ConfigureHttpClientForGeocoding(client, settings!);
        });

        return services;
    }

    private static void ValidateMonobankConfiguration(MonobankSettings? settings)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(settings?.BaseUrl))
            validationErrors.Add("BaseUrl", new List<string> { "Monobank base URL is required" });

        if (!string.IsNullOrWhiteSpace(settings?.BaseUrl) && !Uri.IsWellFormedUriString(settings?.BaseUrl, UriKind.Absolute))
            validationErrors.Add("BaseUrl", new List<string> { "Monobank base URL must be a valid absolute URL" });

        if (string.IsNullOrWhiteSpace(settings?.Token))
            validationErrors.Add("Token", new List<string> { "Monobank API token is required" });

        if (string.IsNullOrWhiteSpace(settings?.WebhookSecret))
            validationErrors.Add("WebhookSecret", new List<string> { "Monobank webhook secret is required" });

        if (string.IsNullOrWhiteSpace(settings?.WebhookBaseUrl))
            validationErrors.Add("WebhookBaseUrl", new List<string> { "Webhook base URL is required" });

        if (!string.IsNullOrWhiteSpace(settings?.WebhookBaseUrl) && !Uri.IsWellFormedUriString(settings.WebhookBaseUrl, UriKind.Absolute))
            validationErrors.Add("WebhookBaseUrl", new List<string> { "Webhook base URL must be a valid absolute URL" });

        if (validationErrors.Any())
            throw new Exception("Invalid Monobank configuration");
    }

    private static void ConfigureHttpClientForMonobankService(HttpClient client, MonobankSettings settings)
    {
        try
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/'));
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Token", settings.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "BookIt/1.0");
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Monobank", "Failed to configure HTTP client", ex);
        }
    }

    private static void ValidateGoogleOAuthConfiguration(GoogleOAuthSettings? settings)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(settings?.ClientId))
            validationErrors.Add("ClientId", new List<string> { "Google OAuth Client ID is required" });

        if (string.IsNullOrWhiteSpace(settings?.ClientSecret))
            validationErrors.Add("ClientSecret", new List<string> { "Google OAuth Client Secret is required" });

        if (string.IsNullOrWhiteSpace(settings?.RedirectUri))
            validationErrors.Add("RedirectUri", new List<string> { "Google OAuth Redirect URI is required" });

        if (!string.IsNullOrWhiteSpace(settings?.RedirectUri) && !Uri.IsWellFormedUriString(settings?.RedirectUri, UriKind.Absolute))
            validationErrors.Add("RedirectUri", new List<string> { "Google OAuth Redirect URI must be a valid absolute URL" });

        if (validationErrors.Any())
            throw new Exception("Invalid Geoogle OAuth configuration");
    }

    private static void ConfigureHttpClientForGoogleAuth(HttpClient client, GoogleOAuthSettings settings)
    {
        try
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "BookIt/1.0");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Google Auth", "Failed to configure HTTP client", ex);
        }
    }

    private static void ValidateGeocodingConfiguration(GeocodingSettings? settings)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(settings?.BaseUrl))
            validationErrors.Add("BaseUrl", new List<string> { "Geocoding base URL is required" });

        if (string.IsNullOrWhiteSpace(settings?.Host))
            validationErrors.Add("Host", new List<string> { "Geocoding host is required" });

        if (string.IsNullOrWhiteSpace(settings?.ApiKey))
            validationErrors.Add("ApiKey", new List<string> { "Geocoding API key is required" });

        if (validationErrors.Any())
            throw new Exception("Invalid Geocoding configuration");
    }

    private static void ConfigureHttpClientForGeocoding(HttpClient client, GeocodingSettings settings)
    {
        try
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/'));
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "BookIt-Application/1.0");
            client.DefaultRequestHeaders.Add("x-rapidapi-host", settings.Host);
            client.DefaultRequestHeaders.Add("x-rapidapi-key", settings.ApiKey);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("HttpClient", "Failed to configure HTTP client for geocoding", ex);
        }
    }
}
