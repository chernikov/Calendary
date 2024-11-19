using Calendary.Model.Messages;
using Calendary.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Calendary.Consumer.Consumers;


public class JobTaskConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public JobTaskConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost", // RabbitMQ хост
            UserName = "user",      // Ім'я користувача
            Password = "password"   // Пароль
        };

        // Створюємо з'єднання асинхронно
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);

        // Створюємо синхронний канал одразу після підключення
        using var channel = await connection.CreateChannelAsync(); // У версії 7.x використовується `CreateChannel` замість `CreateModel`.

        // Декларуємо чергу
        await channel.QueueDeclareAsync(queue: "create-prediction",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // Створюємо споживача
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                Console.WriteLine($"[x] Received: {message}");
                await ProcessMessageAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error: {ex.Message}");
                // Логіка повторної обробки або Dead Letter Queue
            }
        };

        // Підписуємося на чергу
        await channel.BasicConsumeAsync(queue: "create-prediction",
                             autoAck: true,
                             consumer: consumer);

        // Основний цикл
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }


    private async Task ProcessMessageAsync(string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ICalendaryDbContext>();

        var jobData = JsonConvert.DeserializeObject<CreatePredictionMessage>(message);

        if (jobData is null) throw new Exception("Invalid message format");

        // Додати логіку обробки повідомлення:
        // - Оновлення статусу Job/JobTask
        // - Виклик API Replicate
        Console.WriteLine($"Processing JobId: {jobData.JobId}");
    }
}