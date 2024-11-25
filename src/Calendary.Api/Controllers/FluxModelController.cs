using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Calendary.Model;
using Calendary.Model.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Authorize(Roles = "User")]
[Route("api/flux-model")]
public class FluxModelController : BaseUserController
{
    private readonly IFluxModelService _fluxModelService;
    private readonly IReplicateService _replicateService;
    private readonly ITrainingService _trainingService;
    private readonly IMapper _mapper;

    public FluxModelController(IUserService userService, 
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


    [HttpGet]
    public async Task<IActionResult> GetCurrent()
    {
       var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }
        var fluxModel = await _fluxModelService.GetCurrentByUserIdAsync(user.Id);
        if (fluxModel == null)
        {
            return Ok();
        }
        var result = _mapper.Map<FluxModelDto>(fluxModel);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var fluxModel = await _fluxModelService.GetByIdAsync(id);
        if (fluxModel == null)
        {
            return NotFound();
        }
        var result = _mapper.Map<FluxModelDto>(fluxModel);
        return Ok(result);
    }

    // Створення нового FluxModel
    [HttpPost]
    public async Task<IActionResult> Create(CreateFluxModelDto model)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        // Генерація назви моделі
        string randomName = GenerateRandomName();

        var fluxModel = new FluxModel
        {
            UserId = user.Id,
            Gender = model.Gender == "male" ? GenderEnum.Male : GenderEnum.Female,
            Status = "creating",
            Name = randomName,
            IsPaid = true
        };
        await _fluxModelService.CreateAsync(fluxModel);

        var entity = await _fluxModelService.GetByIdAsync(fluxModel.Id);

        var result = _mapper.Map<FluxModelDto>(entity);
        return CreatedAtAction(nameof(GetById), new { id = fluxModel.Id }, result);
    }



    // Оновлення статусу FluxModel
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var fluxModel = await _fluxModelService.GetByIdAsync(id);
        if (fluxModel == null)
        {
            return NotFound();
        }
        fluxModel.Status = status;
        await _fluxModelService.UpdateStatusAsync(fluxModel);
        var result = _mapper.Map<FluxModelDto>(fluxModel);
        return Ok(result);
    }


    [HttpPost("generate")]
    public async Task<IActionResult> GenerateModel(GenerateModelRequest request)
    {
        var fluxModel = await _fluxModelService.GetByIdAsync(request.Id);
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
        var trainModelRequestInput = GetTrainingRequest(fluxModel.ArchiveUrl!);
       
        var trainingResponse = await _replicateService.TrainModelAsync(fluxModel.ReplicateId, trainModelRequestInput);

        await _trainingService.SaveAsync(fluxModel.Id, trainingResponse);

        fluxModel.Status = "Inprocess";   
        await _fluxModelService.UpdateStatusAsync(fluxModel);

        return Ok(new { Message = "Model created and training started successfully." });
    }


    // Оновлення статусу FluxModel
    [HttpPost("archive/{id}")]
    public async Task<IActionResult> UpdateStatus(int id)
    {
        var fluxModel = await _fluxModelService.GetByIdAsync(id);
        if (fluxModel == null)
        {
            return NotFound();
        }
        await _fluxModelService.ArchiveAsync(fluxModel);
        return Ok();
    }


    private string GenerateRandomName()
    {
        // Списки кольорів та істот
        string[] colors = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Black", "White", "Gray" };
        string[] creatures = { "Tiger", "Eagle", "Dolphin", "Dragon", "Unicorn", "Lion", "Wolf", "Bear", "Hawk", "Fox" };

        var random = new Random();
        string color = colors[random.Next(colors.Length)];
        string creature = creatures[random.Next(creatures.Length)];

        return $"{color} {creature}";
    }


    private static TrainModelRequestInput GetTrainingRequest(string archiveUrl)
    {
        return new TrainModelRequestInput()
        {
            Steps = 1000,
            LoraRank = 16,
            Optimizer = "adamw8bit",
            BatchSize = 1,
            Resolution = "512,768,1024",
            Autocaption = true,
            AutocaptionPrefix = "a photo of TOK",
            InputImages = $"https://calendary.com.ua/{archiveUrl}",
            TriggerWord = "TOK",
            LearningRate = 0.0004,
            WandbProject = "flux_train_replicate",
            WandbSaveInterval = 100,
            WandbSampleInterval = 100,
            CaptionDropoutRate = 0.05,
            CacheLatentsToDisk = false
        };
    }
}