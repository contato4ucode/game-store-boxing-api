using GameStore.Messaging.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace GameStore.Messaging.Services;

public class RabbitMQProducer : IMessageProducer
{
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQProducer> _logger;

    public RabbitMQProducer(IModel channel, ILogger<RabbitMQProducer> logger)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        try
        {
            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.BasicPublish(exchange, routingKey, null, messageBody);
            _logger.LogInformation("Message published to exchange {Exchange} with routing key {RoutingKey}", exchange, routingKey);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to exchange {Exchange} with routing key {RoutingKey}", exchange, routingKey);
            throw;
        }
    }
}
