using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BookIt.API.Extensions;

public static class BearerJwtTokenRegistrationExtension
{
    private const string DEFAULT_AUTHENTICATION_SCHEME = "Bearer";

    public static AuthenticationBuilder AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var authBuilder = services.AddAuthentication(DEFAULT_AUTHENTICATION_SCHEME)
            .AddJwtBearer(DEFAULT_AUTHENTICATION_SCHEME, options =>
            {
                var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                             ?? config.GetRequiredSection("JWT:Secret").Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config.GetRequiredSection("JWT:Issuer").Value,
                    ValidAudience = config.GetRequiredSection("JWT:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!))
                };
            });

        return authBuilder;
    }
}