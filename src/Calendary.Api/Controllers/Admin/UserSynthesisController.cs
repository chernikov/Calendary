﻿using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/user/{userId:int}/synthesis")]
public class UserSynthesisController : BaseAdminController
{
    private readonly ISynthesisService _synthesisService;
    private readonly IFluxModelService _fluxModelService;
    private readonly IReplicateService _replicateService;
    private readonly ITrainingService _trainingService;
    private readonly IMapper _mapper;

    public UserSynthesisController(IUserService userService,
        ISynthesisService synthesisService,
        IFluxModelService fluxModelService,
        IReplicateService replicateService,
        ITrainingService trainingService,
        IMapper mapper) : base(userService)
    {
        _synthesisService = synthesisService;
        _fluxModelService = fluxModelService;
        _replicateService = replicateService;
        _trainingService = trainingService;
        _mapper = mapper;
    }


    // GET: api/admin/user/{userId:int}/synthesis
    [HttpGet("{trainingId:int}")]
    public async Task<IActionResult> GetSynthesisesByTrainingId(int trainingId)
    {
        var synthesises = await _synthesisService.GetByTrainingIdAsync(trainingId);
        var result = _mapper.Map<List<SynthesisDto>>(synthesises);
        return Ok(synthesises);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAndRun(int userId, [FromBody] CreateSynthesisDto createSynthesisDto)
    {
        // Отримуємо модель
        var model = await _fluxModelService.GetByIdAsync(createSynthesisDto.FluxModelId);
        if (model == null)
        {
            return NotFound($"Model with ID {createSynthesisDto.FluxModelId} not found.");
        }

        var training = await _trainingService.GetByIdAsync(createSynthesisDto.TrainingId);

        if (training == null)
        {
            return NotFound($"Training with ID {createSynthesisDto.TrainingId} not found.");
        }

        // Створюємо тест
        var synthesis = await _synthesisService.CreateAsync(createSynthesisDto.PromptId,
                training.Id,
                createSynthesisDto.Text,
                createSynthesisDto.Seed);

        try
        {
            // Sending request to ReplicateService to generate image
            var imageRequest = GenerateImageInput.GetImageRequest(synthesis.Text, synthesis.Seed);
            var result = await _replicateService.GenerateImageAsync(model.Version, imageRequest);

            if (result is not null && result.Output.Count > 0)
            {
                var info = await _replicateService.GeGenerateImageStatusAsync(result.Id);
                var imagePath = result.Output[0];
                // Updating task status and result
                var seed = info.ExtractSeedFromLogs();
                synthesis.ReplicateId = result.Id;
                synthesis.OutputSeed = seed;
                synthesis.Status = "completed";
                synthesis.ProcessedImageUrl = imagePath;
                synthesis.ImageUrl = await _replicateService.DownloadAndSaveImageAsync(imagePath);

                await _synthesisService.UpdateResultAsync(synthesis);
            }

            var output = _mapper.Map<SynthesisDto>(synthesis);
            return Ok(output);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error while running Job: {ex.Message}");
        }
    }
}
