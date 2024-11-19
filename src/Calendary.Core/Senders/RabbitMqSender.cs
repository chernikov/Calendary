using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace Calendary.Core.Senders;

public interface IRabbitMqSender
{
    Task SendMessageAsync(string queueName, string message);
}

public class RabbitMqSender : IRabbitMqSender
{
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;

    public RabbitMqSender(IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMQ");
        _hostName = rabbitMqSettings["Host"];
        _userName = rabbitMqSettings["User"];
        _password = rabbitMqSettings["Password"];
    }

    public async Task SendMessageAsync(string queueName, string message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            UserName = _userName,
            Password = _password
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Декларуємо чергу, якщо вона ще не існує
        await channel.QueueDeclareAsync(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // Конвертуємо повідомлення в байти
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            body: body);

        Console.WriteLine($"[x] Sent message to queue '{queueName}': {message}");
    }
}
