using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Calendary.Model;
using Calendary.Model.Messages;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Calendary.Consumer.Consumers;

public class CreatePredictionConsumer : IQueueConsumer
{
    private readonly ILogger<CreatePredictionConsumer> _logger;
    private readonly IReplicateService _replicateService;
    private readonly IJobTaskService _jobTaskService;
    private readonly IJobService _jobService;
    private readonly IFluxModelService _fluxModelService;
    private readonly IPromptService _promptService;

    public CreatePredictionConsumer(
        ILogger<CreatePredictionConsumer> logger,
        IReplicateService replicateService,
        IJobTaskService jobTaskService,
        IJobService jobService,
        IFluxModelService fluxModelService,
        IPromptService promptService)
    {
        _logger = logger;
        _replicateService = replicateService;
        _jobTaskService = jobTaskService;
        _jobService = jobService;
        _fluxModelService = fluxModelService;
        _promptService = promptService;
    }

    public async Task ProcessMessageAsync(string message)
    {
        _logger.LogInformation("Processing message from create-prediction: {Message}", message);

        // Десеріалізація JSON у TaskDto
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

        
    }

   
}