namespace GameStore.Domain.Interfaces.Services;

public interface IRedisCacheService
{
    Task<T> GetCacheValueAsync<T>(string key);
    Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveCacheValueAsync(string key);
}
