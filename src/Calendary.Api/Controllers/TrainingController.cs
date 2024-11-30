using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;



[Route("api/[controller]")]
[ApiController]
public class TrainingController : BaseUserController
{
    private readonly ITrainingService _trainingService;
    private readonly IReplicateService _replicateService;

    public TrainingController(
        ITrainingService trainingService,
        IReplicateService replicateService,
        IUserService userService) : base(userService)
    {
        _trainingService = trainingService;
        _replicateService = replicateService;
    }

    /// <summary>
    /// Отримує статус тренування за FluxModelId.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStatus(int id)
    {
        // Знаходимо тренування
        var training = await _trainingService.GetByIdAsync(id);
        if (training == null)
        {
            return NotFound($"Training with ID {id} not found.");
        }

        // Отримуємо статус тренування
        var status = await _replicateService.GetTrainingStatusAsync(training.ReplicateId);
        return Ok(status);
    }

    /// <summary>
    /// Скасовує тренування за id.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelTraining(int id)
    {
        // Знаходимо тренування
        var training = await _trainingService.GetByIdAsync(id);
        if (training == null)
        {
            return NotFound($"Training with ID {id} not found.");
        }

        // Скасовуємо тренування через Replicate API
        await _replicateService.CancelTrainingAsync(training.ReplicateId);

        // Оновлюємо статус у базі даних
        await _trainingService.UpdateStatusAsync(training.Id, "canceled");

        return Ok(new { Message = "Training canceled successfully." });
    }
}
