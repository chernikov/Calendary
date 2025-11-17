using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/synthesis")]
public class SynthesisController : BaseAdminController
{
    private readonly ITrainingService _trainingService;
    private readonly ISynthesisService _synthesisService;
    private readonly IFluxModelService _fluxModelService;
    private readonly IReplicateService _replicateService;
    private readonly IMapper _mapper;

    public SynthesisController(IUserService userService,
        IFluxModelService fluxModelService,
        ITrainingService trainingService,
        ISynthesisService synthesisService, 
        IReplicateService replicateService,
        IMapper mapper) : base(userService)
    {
        _trainingService = trainingService;
        _synthesisService = synthesisService;
        _fluxModelService = fluxModelService;
        _replicateService = replicateService;
        _mapper = mapper;
    }


    [HttpGet("{idPrompt:int}")]
    public async Task<IActionResult> Get(int idPrompt)
    {
        var synthesises = await _synthesisService.GetByPromptIdAsync(idPrompt);
        var result =  _mapper.Map<IEnumerable<SynthesisDto>>(synthesises);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSynthesisDto createSynthesisDto)
    {
        // Отримуємо модель
        var model = await _fluxModelService.GetByIdAsync(createSynthesisDto.FluxModelId);
        if (model == null)
        {
            return NotFound($"Model with ID {createSynthesisDto.FluxModelId} not found.");
        }

        var trainings = await _trainingService.GetByModelIdAsync(createSynthesisDto.FluxModelId);
        // Вибираємо останній доступний TrainingId
        var latestTraining = trainings.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
        if (latestTraining == null)
        {
            return NotFound($"No available training for model ID {createSynthesisDto.FluxModelId}.");
        }

        // Створюємо тест
        var entry = await _synthesisService.CreateAsync(createSynthesisDto.PromptId, 
                latestTraining.Id, 
                createSynthesisDto.Text,
                createSynthesisDto.Seed);
        var result = _mapper.Map<SynthesisDto>(entry);

        return Ok(result);
    }


    [HttpGet("run/{id:int}")]
    public async Task<IActionResult> Run(int id)
    {
        try
        {
            var synthesis = await _synthesisService.GetByIdAsync(id);

            if (synthesis is null)
            {
                return NotFound($"Synthesis with ID {id} not found.");
            }

            var fluxModel = await _fluxModelService.GetByTrainingIdAsync(synthesis.TrainingId);
            if (fluxModel is null)
            {
                return NotFound($"FluxModel not found.");
            }

            if (synthesis.Status == "prepared")
            {
                // Sending request to ReplicateService to generate image
                var imageRequest = GenerateImageInput.GetImageRequest(synthesis.Text, synthesis.Seed);
                var predictionId = await _replicateService.StartImageGenerationAsync(fluxModel.Version, imageRequest);
                var result = await _replicateService.GenerateImageAsync(predictionId);

                if (result is not null && result.Output.Count > 0)
                {
                    var imagePath = result.Output[0];
                    // Updating task status and result
                    var seed = result.ExtractSeedFromLogs();
                    synthesis.ReplicateId = result.Id;
                    synthesis.OutputSeed = seed; 
                    synthesis.Status = "completed";
                    synthesis.ProcessedImageUrl = imagePath;
                    synthesis.ImageUrl = await _replicateService.DownloadAndSaveImageAsync(imagePath);

                    await _synthesisService.UpdateResultAsync(synthesis);
                }
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
