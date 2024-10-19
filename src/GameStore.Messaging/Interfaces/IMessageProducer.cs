namespace GameStore.Messaging.Interfaces;

public interface IMessageProducer
{
    Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class;
}
