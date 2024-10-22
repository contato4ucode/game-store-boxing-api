using GameStore.Domain.Interfaces.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace GameStore.SharedServices.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;
    private readonly JsonSerializerSettings _jsonSettings;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
        _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };
    }

    public async Task<T> GetCacheValueAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(value, _jsonSettings);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Erro ao desserializar o valor do cache: {ex.Message}");
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao acessar o Redis: {ex.Message}");
            throw;
        }
    }

    public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var jsonValue = JsonConvert.SerializeObject(value, _jsonSettings);
            if (expiration.HasValue)
            {
                await _database.StringSetAsync(key, jsonValue, expiration);
            }
            else
            {
                await _database.StringSetAsync(key, jsonValue);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao definir o valor do cache: {ex.Message}");
            throw;
        }
    }

    public async Task RemoveCacheValueAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao remover o valor do cache: {ex.Message}");
            throw;
        }
    }
}
