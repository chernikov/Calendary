using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Calendary.Repos;
using Calendary.Consumer;
using Microsoft.Extensions.Configuration;
using Calendary.Consumer.Consumers;




IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {

        var host = context.Configuration["RabbitMQ:Host"];
        var user = context.Configuration["RabbitMQ:User"];
        var password = context.Configuration["RabbitMQ:Password"];

        Console.WriteLine($"{host} {user} {password}");
        // Підключення до бази даних
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Can't find connection string with name DefaultConnection");
        Console.WriteLine(connectionString);
        services.AddCalendaryRepositories(connectionString);

        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddTransient<CreatePredictionConsumer>();
        services.AddHostedService<RabbitMqHostService>();
    })
    .Build();

await host.RunAsync();