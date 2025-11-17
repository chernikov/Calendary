using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/user/{userId:int}/training")]
public class UserTrainingController : BaseAdminController
{
    private readonly IFluxModelService _fluxModelService;
    private readonly IReplicateService _replicateService;
    private readonly ITrainingService _trainingService;
    private readonly IMapper _mapper;

    public UserTrainingController(IUserService userService, 
        IFluxModelService fluxModelService, 
        IReplicateService replicateService, 
        ITrainingService trainingService,
        IMapper mapper) : base(userService)
    {
        _fluxModelService = fluxModelService;
        _replicateService = replicateService;
        _trainingService = trainingService;
        _mapper = mapper;
    }

    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var training = await _trainingService.GetByIdAsync(id);
        if (training == null)
        {
            return NotFound($"Training with ID {id} not found.");
        }

        var result = _mapper.Map<TrainingDto>(training);

        return Ok(result);
    }

    [HttpGet("generate/{fluxModelId:int}")]
    public async Task<IActionResult> GenerateTraining(int userId, int fluxModelId)
    {
        var fluxModel = await _fluxModelService.GetByIdAsync(fluxModelId);
        if (fluxModel == null)
        {
            return NotFound();
        }

        if (fluxModel.ArchiveUrl == null)
        {
            return BadRequest("Archive isn't prepared");
        }

        // Виклик створення моделі в Replicate
        var random = new Random();
        var randomDigits = random.Next(100, 1000); // Генерує число від 100 до 999 (включно)
        var replicateName = $"chernikov_api_flux_{fluxModel.Id}_{randomDigits}";

        var createModelResponse = await _replicateService.CreateModelAsync(replicateName, fluxModel.Name);

        // Записуємо ReplicateId у модель
        fluxModel.ReplicateId = $"{createModelResponse.Owner}/{createModelResponse.Name}";
        await _fluxModelService.UpdateReplicateIdAsync(fluxModel);

        // Запуск тренування
        var trainModelRequestInput = TrainModelRequestInput.GetTrainingRequest(fluxModel.ArchiveUrl!);

        var trainingResponse = await _replicateService.TrainModelAsync(fluxModel.ReplicateId, trainModelRequestInput);

        await _trainingService.SaveAsync(fluxModel.Id, trainingResponse);

        fluxModel.Status = "inprocess";
        await _fluxModelService.UpdateStatusAsync(fluxModel);

        return Ok(new { Message = "Model created and training started successfully." });
    }

    /// <summary>
    /// Отримує статус тренування за id.
    /// </summary>
    [HttpGet("status/{id:int}")]
    public async Task<IActionResult> GetStatus(int id)
    {
        // Знаходимо тренування
        var training = await _trainingService.GetByIdAsync(id);
        if (training == null)
        {
            return NotFound($"Training with ID {id} not found.");
        }
        try
        {
            // Отримуємо статус тренування з Replicate
            var replicateResponse = await _replicateService.GetTrainingStatusAsync(training.ReplicateId);

            // Виконуємо оновлення запису у базі даних
            await _trainingService.UpdateStatusAsync(training.Id, replicateResponse.Status);
        } 
        catch (Exception ex)
        {
            await _trainingService.UpdateStatusAsync(training.Id, "unknown");
        }
        var savedTraining = await _trainingService.GetByIdAsync(id);
        var result = _mapper.Map<TrainingDto>(savedTraining);
        return Ok(result);
    }
}
