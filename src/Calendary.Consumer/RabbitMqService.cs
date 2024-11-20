using Calendary.Core.Senders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Consumer;

public interface IRabbitMqService : IDisposable
{
    IChannel GetChannel();
}

public class RabbitMqService : IRabbitMqService
{
    private IConnection _connection;
    private IChannel _channel;

    public RabbitMqService(IConfiguration configuration)
    {
        Task.Run(async () => await Init(configuration)).Wait();
    }
                           
    private async Task Init(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"]!,
            UserName = configuration["RabbitMQ:User"]!,
            Password = configuration["RabbitMQ:Password"]!
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public IChannel GetChannel()
    {
        return _channel;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
