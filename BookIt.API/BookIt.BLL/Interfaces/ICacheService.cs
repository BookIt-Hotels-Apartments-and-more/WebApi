namespace BookIt.BLL.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task<string?> GetStringAsync(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
    Task<TimeSpan?> GetTimeToLiveAsync(string key);
}