using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Calendary.Repos;
using Calendary.Consumer;
using Microsoft.Extensions.Configuration;
using Calendary.Consumer.Consumers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Підключення до бази даних
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        services.AddCalendaryRepositories(connectionString);

        // Реєстрація RabbitMQ Consumer
        services.AddHostedService<JobTaskConsumer>();
    })
    .Build();

await host.RunAsync();