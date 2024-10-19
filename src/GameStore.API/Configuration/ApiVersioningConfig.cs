using Asp.Versioning;

namespace GameStore.API.Configuration;

public static class ApiVersioningConfig
{
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        var apiVersioningBuilder = ConfigureApiVersioning(services);
        ConfigureApiExplorer(apiVersioningBuilder);

        return services;
    }

    private static IApiVersioningBuilder ConfigureApiVersioning(IServiceCollection services)
    {
        return services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });
    }

    private static void ConfigureApiExplorer(IApiVersioningBuilder apiVersioningBuilder)
    {
        apiVersioningBuilder.AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }
}
