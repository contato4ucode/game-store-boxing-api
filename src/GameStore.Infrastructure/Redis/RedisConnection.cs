using StackExchange.Redis;

namespace GameStore.Infrastructure.Redis;

public class RedisConnection
{
    private static Lazy<ConnectionMultiplexer> lazyConnection;

    public static void Initialize(ConfigurationOptions configurationOptions)
    {
        lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            configurationOptions.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configurationOptions);
        });
    }

    public static ConnectionMultiplexer Connection => lazyConnection.Value;
}
