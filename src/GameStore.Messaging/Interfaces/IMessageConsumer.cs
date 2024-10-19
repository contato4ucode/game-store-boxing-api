namespace GameStore.Messaging.Interfaces;

public interface IMessageConsumer
{
    Task ConsumeAsync();
}
