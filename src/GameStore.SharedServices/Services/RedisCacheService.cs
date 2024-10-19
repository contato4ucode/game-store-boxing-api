using GameStore.Domain.Interfaces.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace GameStore.SharedServices.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T> GetCacheValueAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? JsonConvert.DeserializeObject<T>(value) : default;
    }

    public async Task SetCacheValueAsync<T>(string key, T value)
    {
        await _database.StringSetAsync(key, JsonConvert.SerializeObject(value));
    }

    public async Task RemoveCacheValueAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}
