using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace GameStore.API.Configuration;

public static class HealthCheckConfig
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var redisHost = configuration["RedisSettings:Host"];
        var redisPort = configuration["RedisSettings:Port"];
        var postgreConnectionString = configuration.GetSection("DatabaseSettings:DefaultConnection").Value;
        var rabbitMqHostName = configuration["RabbitMqSettings:HostName"];
        var rabbitMqPort = configuration["RabbitMqSettings:Port"];
        var rabbitMqUserName = configuration["RabbitMqSettings:UserName"];
        var rabbitMqPassword = configuration["RabbitMqSettings:Password"];
        var azureBlobConnectionString = Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");
        var azureBlobContainerName = configuration["AzureBlobStorageSettings:ContainerName"];

        var redisConfigurationOptions = new ConfigurationOptions
        {
            EndPoints = { $"{redisHost}:{redisPort}" },
            AbortOnConnectFail = false
        };

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(redisConfigurationOptions);
        });

        services.AddHealthChecks()
            .AddNpgSql(postgreConnectionString)
            .AddRedis($"{redisHost}:{redisPort}", name: "redis")
            .AddRabbitMQ(rabbitConnectionString: $"amqp://{rabbitMqUserName}:{rabbitMqPassword}@{rabbitMqHostName}:{rabbitMqPort}",
                         name: "rabbitmq");

        services.AddHealthChecksUI()
            .AddInMemoryStorage();

        return services;
    }

    public static IApplicationBuilder UseHealthCheckConfiguration(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            endpoints.MapHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
                options.ApiPath = "/health-ui-api";
            });
        });

        return app;
    }
}
