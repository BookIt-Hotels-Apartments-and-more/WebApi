using BookIt.BLL.Interfaces;
using BookIt.BLL.Services;
using BookIt.DAL.Configuration.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BookIt.API.Extensions;

public static class RedisRegistrationExtension
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var redisSettings = serviceProvider.GetRequiredService<IOptions<RedisSettings>>().Value;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Redis");

            if (string.IsNullOrEmpty(redisSettings.ConnectionString))
            {
                logger.LogError("Redis connection string is missing. Please set REDIS_CONNECTION_STRING environment variable.");
                throw new InvalidOperationException("Redis connection string is required");
            }

            try
            {
                return ConnectionMultiplexer.Connect(redisSettings.ConnectionString) ?? throw new Exception("Connection is null");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
                throw new InvalidOperationException("Failed to establish Redis connection", ex);
            }
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddStackExchangeRedisCache(options =>
        {
            var serviceProvider = services.BuildServiceProvider();
            var redisSettings = serviceProvider.GetRequiredService<IOptions<RedisSettings>>().Value;
            var multiplexer = serviceProvider.GetRequiredService<IConnectionMultiplexer>();

            options.ConnectionMultiplexerFactory = async () => await Task.FromResult(multiplexer);
            options.InstanceName = $"{redisSettings.InstanceName}:";
        });

        return services;
    }
}
