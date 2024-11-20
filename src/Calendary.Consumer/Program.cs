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

        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddTransient<CreatePredictionConsumer>();
        services.AddHostedService<RabbitMqHostService>();
    })
    .Build();

await host.RunAsync();