using GameStore.Messaging.Interfaces;

namespace GameStore.Messaging.Services;

public class RabbitMQHostedServiceConsumer : BackgroundService
{
    private readonly IEnumerable<IMessageConsumer> _consumers;
    private readonly ILogger<RabbitMQHostedServiceConsumer> _logger;

    public RabbitMQHostedServiceConsumer(IEnumerable<IMessageConsumer> consumers, ILogger<RabbitMQHostedServiceConsumer> logger)
    {
        _consumers = consumers ?? throw new ArgumentNullException(nameof(consumers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = _consumers.Select(async consumer =>
        {
            try
            {
                await consumer.ConsumeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while consuming messages.");
            }
        });

        await Task.WhenAll(tasks);
    }
}
