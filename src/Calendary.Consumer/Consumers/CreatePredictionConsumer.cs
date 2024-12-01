using Calendary.Model.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Calendary.Consumer.Consumers;

public class CreatePredictionConsumer : IQueueConsumer
{
    private readonly ILogger<CreatePredictionConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CreatePredictionConsumer(
        ILogger<CreatePredictionConsumer> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task ProcessMessageAsync(string message)
    {
        _logger.LogInformation("Processing message from create-prediction: {Message}", message);

        JobTaskMessage? taskMessage;
        try
        {
            taskMessage = JsonSerializer.Deserialize<JobTaskMessage>(message);
            if (taskMessage == null)
            {
                _logger.LogWarning("Deserialized task is null.");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message.");
            return;
        }

        // Формуємо URL для API
        var apiUrl = _configuration["API:runTask"];
        if (string.IsNullOrEmpty(apiUrl))
        {
            _logger.LogError("API:runTask is not configured.");
            return;
        }

        var fullUrl = $"{apiUrl}/{taskMessage.Id}";

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(fullUrl);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully triggered runTask API for Task ID: {TaskId}", taskMessage.Id);
            }
            else
            {
                _logger.LogWarning("Failed to trigger runTask API for Task ID: {TaskId}. Status Code: {StatusCode}",
                    taskMessage.Id, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while triggering runTask API for Task ID: {TaskId}", taskMessage.Id);
        }
    }
}