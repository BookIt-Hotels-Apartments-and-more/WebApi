using BookIt.DAL.Configuration.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookIt.DAL.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConnectionStrings>(options =>
        {
            configuration.GetSection(ConnectionStrings.SectionName).Bind(options);

            var defaultConnection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(defaultConnection))
                options.DefaultConnection = defaultConnection;

            var azureBlobStorage = Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(azureBlobStorage))
                options.AzureBlobStorage = azureBlobStorage;
        });

        services.Configure<JwtSettings>(options =>
        {
            configuration.GetSection(JwtSettings.SectionName).Bind(options);

            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
            if (!string.IsNullOrEmpty(jwtSecret))
                options.Secret = jwtSecret;
        });

        services.Configure<GoogleOAuthSettings>(options =>
        {
            configuration.GetSection(GoogleOAuthSettings.SectionName).Bind(options);

            var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            if (!string.IsNullOrEmpty(clientId))
                options.ClientId = clientId;

            var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
            if (!string.IsNullOrEmpty(clientSecret))
                options.ClientSecret = clientSecret;
        });

        services.Configure<EmailSMTPSettings>(options =>
        {
            configuration.GetSection(EmailSMTPSettings.SectionName).Bind(options);

            var password = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD");
            if (!string.IsNullOrEmpty(password))
                options.Password = password;
        });

        services.Configure<UrlSettings>(options =>
        {
            configuration.GetSection(UrlSettings.SectionName).Bind(options);

            var clientUrl = Environment.GetEnvironmentVariable("CLIENT_URL");
            if (!string.IsNullOrEmpty(clientUrl))
                options.ClientUrl = clientUrl;
        });

        services.Configure<AppSettings>(options =>
        {
            configuration.GetSection(AppSettings.SectionName).Bind(options);

            var baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
            if (!string.IsNullOrEmpty(baseUrl))
                options.BaseUrl = baseUrl;
        });

        services.Configure<MonobankSettings>(options =>
        {
            configuration.GetSection(MonobankSettings.SectionName).Bind(options);

            var token = Environment.GetEnvironmentVariable("MONOBANK_TOKEN");
            if (!string.IsNullOrEmpty(token))
                options.Token = token;

            var webhookSecret = Environment.GetEnvironmentVariable("MONOBANK_WEBHOOK_SECRET");
            if (!string.IsNullOrEmpty(webhookSecret))
                options.WebhookSecret = webhookSecret;
        });

        services.Configure<GeocodingSettings>(options =>
        {
            configuration.GetSection(GeocodingSettings.SectionName).Bind(options);

            var apiKey = Environment.GetEnvironmentVariable("GEOCODING_API_KEY");
            if (!string.IsNullOrEmpty(apiKey))
                options.ApiKey = apiKey;
        });

        services.Configure<GeminiAISettings>(options =>
        {
            configuration.GetSection(GeminiAISettings.SectionName).Bind(options);

            var apiKey = Environment.GetEnvironmentVariable("GEMINI_AI_API_KEY");
            if (!string.IsNullOrEmpty(apiKey))
                options.ApiKey = apiKey;
        });
    }
}
