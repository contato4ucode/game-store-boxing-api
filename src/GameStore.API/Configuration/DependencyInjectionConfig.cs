using FluentValidation;
using GameStore.Domain.Interfaces;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Repositories;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Notifications;
using GameStore.Identity.Extensions;
using GameStore.Identity.Interfaces;
using GameStore.Identity.Services;
using GameStore.Infrastructure.Repositories;
using GameStore.Infrastructure.UoW;
using GameStore.Messaging.Configurations;
using GameStore.Messaging.Interfaces;
using GameStore.Messaging.Services;
using GameStore.SharedServices.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameStore.API.Configuration;

public static class DependencyInjectionConfig
{
    public static void AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterManualServices(services);

        services.Scan(scan => scan
            .FromAssemblies(
                typeof(INotifier).Assembly,
                typeof(IRepository<>).Assembly,
                typeof(BaseService).Assembly,
                typeof(IAspNetUser).Assembly,
                typeof(IMessageProducer).Assembly,
                typeof(IMessageConsumer).Assembly,
                typeof(IValidator<>).Assembly
            )
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service") ||
                                                         type.Name.EndsWith("Repository") ||
                                                         type.Name.EndsWith("UnitOfWork") ||
                                                         type.Name.EndsWith("Validation")))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Producer") ||
                                                         type.Name.EndsWith("Consumer") ||
                                                         typeof(IHostedService).IsAssignableFrom(type)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
        );

        AddMessagingServices(services, configuration);

        services.AddTransient(provider =>
        {
            var message = "Default message";
            return new Notification(message, NotificationType.Information);
        });
    }

    private static void RegisterManualServices(IServiceCollection services)
    {
        services.TryAddScoped<INotifier, Notifier>();
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        services.TryAddScoped<IBoxRepository, BoxRepository>();
        services.TryAddScoped<IOrderRepository, OrderRepository>();
        services.TryAddScoped<IProductRepository, ProductRepository>();
        services.TryAddScoped<IAspNetUser, AspNetUser>();
        services.TryAddScoped<IAuthService, AuthService>();
        services.TryAddScoped<RoleManager<IdentityRole>>();
        services.AddAuthorization();
        services.TryAddSingleton<IRedisCacheService, RedisCacheService>();
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddSingleton<IMessageProducer, RabbitMQProducer>();
    }

    private static void AddMessagingServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMQ(configuration);
    }
}
