using RabbitMQ.Client;

namespace GameStore.Messaging.Configurations;

public static class RabbitMQConfig
{
    public static void AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMQSettings = configuration.GetSection("RabbitMqSettings").Get<RabbitMQSettings>();

        var factory = new ConnectionFactory()
        {
            HostName = rabbitMQSettings.HostName,
            Port = rabbitMQSettings.Port,
            UserName = rabbitMQSettings.UserName,
            Password = rabbitMQSettings.Password
        };

        services.AddSingleton(factory);

        services.AddSingleton(sp =>
        {
            var connFactory = sp.GetRequiredService<ConnectionFactory>();
            return connFactory.CreateConnection();
        });

        services.AddSingleton<IModel>(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("box_exchange", ExchangeType.Direct, true);
            channel.ExchangeDeclare("order_exchange", ExchangeType.Direct, true);
            channel.ExchangeDeclare("product_exchange", ExchangeType.Direct, true);

            channel.QueueDeclare("box_queue", true, false, false, null);
            channel.QueueDeclare("order_queue", true, false, false, null);
            channel.QueueDeclare("product_queue", true, false, false, null);

            channel.QueueBind("box_queue", "box_exchange", "box_routingKey");
            channel.QueueBind("order_queue", "order_exchange", "order_routingKey");
            channel.QueueBind("product_queue", "product_exchange", "product_routingKey");

            return channel;
        });
    }
}
