using Microsoft.Extensions.Logging;

namespace Calendary.Consumer.Consumers;

public class CreatePredictionConsumer : IQueueConsumer
{
    private readonly ILogger<CreatePredictionConsumer> _logger;

    public CreatePredictionConsumer(ILogger<CreatePredictionConsumer> logger)
    {
        _logger = logger;
    }

    public async Task ProcessMessageAsync(string message)
    {
        _logger.LogInformation("Processing message from create-prediction: {Message}", message);
        // Додайте бізнес-логіку обробки повідомлення тут
    }
}