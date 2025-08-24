namespace BookIt.API.Extensions;

public static class CorsSecurityPolicyExtension
{
    private const string CORS_POLICY_NAME = "ALLOW_BOOKIT_CLIENT_CSP";

    public static (IServiceCollection services, string cspName) AddCorsSecurityPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CORS_POLICY_NAME, p =>
                p.WithOrigins
                (
                    Environment.GetEnvironmentVariable("CLIENT_URL") ?? string.Empty,
                    Environment.GetEnvironmentVariable("CLIENT_URL")?.Replace("http", "https") ?? string.Empty,
                    Environment.GetEnvironmentVariable("CLIENT_URL")?.Replace("https", "http") ?? string.Empty
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        return (services, CORS_POLICY_NAME);
    }
}
