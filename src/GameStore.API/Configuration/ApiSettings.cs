using Asp.Versioning.ApiExplorer;
using GameStore.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json.Serialization;

namespace GameStore.API.Configuration;

public class ApiSettings
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        configuration = BuildConfiguration(environment);
        services.AddSingleton(configuration);

        ConfigureDatabase(services, configuration);
        ConfigureRedis(services, configuration);
        ConfigureControllers(services);
        ConfigureAdditionalServices(services, configuration);
    }

    private static IConfiguration BuildConfiguration(IWebHostEnvironment environment)
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("DatabaseSettings:DefaultConnection").Value;
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(connectionString));
    }

    private static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
    {
        var redisHost = configuration["RedisSettings:Host"];
        var redisPort = configuration["RedisSettings:Port"];

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { $"{redisHost}:{redisPort}" },
                AbortOnConnectFail = false
            };
            return ConnectionMultiplexer.Connect(configurationOptions);
        });
    }

    private static void ConfigureControllers(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
    }

    private static void ConfigureAdditionalServices(IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureAutomapper();
        services.AddDependencyInjection(configuration);
        services.AddApiVersioningConfiguration();
        services.AddSwaggerConfig();
        services.AddHealthCheckConfiguration(configuration);
        services.AddHttpContextAccessor();
        services.AddIdentityConfig(configuration);
        services.AddAuthorization();
        services.AddAuthentication();
    }

    public void ConfigurePipeline(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwaggerConfig(provider);
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHealthCheckConfiguration();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
