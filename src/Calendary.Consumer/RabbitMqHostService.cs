using Calendary.Consumer.Consumers;
using Calendary.Core.Senders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Calendary.Consumer;

public class RabbitMqHostService : BackgroundService
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqHostService> _logger;

    public RabbitMqHostService(
        IRabbitMqService rabbitMqService,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqHostService> logger)
    {
        _rabbitMqService = rabbitMqService;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _rabbitMqService.GetChannel();
        var queues = _configuration.GetSection("RabbitMQ:Queues").Get<List<string>>();

        foreach (var queue in queues)
        {
            await channel.QueueDeclareAsync(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync +=async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                   await HandleMessageAsync(queue, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {Queue}", queue);
                }
            };

            await channel.BasicConsumeAsync(queue: queue,
                                 autoAck: true,
                                 consumer: consumer);

            _logger.LogInformation("Subscribed to queue: {Queue}", queue);
        }
    }

    private async Task HandleMessageAsync(string queue, string message)
    {
        using var scope = _serviceProvider.CreateScope();
        switch (queue)
        {
            case "create-prediction":
                var createPredictionConsumer = scope.ServiceProvider.GetRequiredService<CreatePredictionConsumer>();
                await createPredictionConsumer.ProcessMessageAsync(message);
                break;

            // Додайте обробку інших черг тут
            default:
                _logger.LogWarning("No handler for queue: {Queue}", queue);
                break;
        }
    }
}
