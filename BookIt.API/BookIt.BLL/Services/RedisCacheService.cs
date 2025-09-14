using BookIt.BLL.Interfaces;
using BookIt.DAL.Configuration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace BookIt.BLL.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly RedisSettings _settings;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(
        IOptions<RedisSettings> settings,
        ILogger<RedisCacheService> logger,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _settings = settings.Value;
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty) return default;

            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting cache key: {key}");
            return default;
        }
    }

    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting cache key {key}");
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serializedValue, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache key: {key}");
        }
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiration = null)
    {
        try
        {
            await _database.StringSetAsync(key, value, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache key: {key}");
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);

        if (cached is not null)
        {
            _logger.LogDebug($"Cache hit for key: {key}");
            return cached;
        }

        _logger.LogDebug($"Cache miss for key: {key}");
        var value = await factory();

        if (value is not null)
        {
            await SetAsync(key, value, expiration);
        }

        return value;
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug($"Removed cache key: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache key: {key}");
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var endpoints = _connectionMultiplexer.GetEndPoints();
            var server = _connectionMultiplexer.GetServer(endpoints.First());

            var keys = server.Keys(pattern: $"*{pattern}*").ToArray();

            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogDebug($"Removed {keys.Length} keys matching pattern: {pattern}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache keys by pattern: {pattern}");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if key exists: {key}");
            return false;
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        try
        {
            return await _database.KeyTimeToLiveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting TTL for key: {key}");
            return null;
        }
    }
}