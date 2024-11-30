using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/test-prompt")]
public class TestPromptController : BaseAdminController
{
    private readonly ITrainingService _trainingService;
    private readonly ITestPromptService _testPromptService;
    private readonly IFluxModelService _fluxModelService;
    private readonly IReplicateService _replicateService;
    private readonly IMapper _mapper;

    public TestPromptController(IUserService userService,
        IFluxModelService fluxModelService,
        ITrainingService trainingService,
        ITestPromptService testPromptService, 
        IReplicateService replicateService,
        IMapper mapper) : base(userService)
    {
        _trainingService = trainingService; 
        _testPromptService = testPromptService;
        _fluxModelService = fluxModelService;
        _replicateService = replicateService;
        _mapper = mapper;
    }


    [HttpGet("{idPrompt:int}")]
    public async Task<IActionResult> Get(int idPrompt)
    {
        var testPrompts = await _testPromptService.GetByPromptIdAsync(idPrompt);
        var result =  _mapper.Map<IEnumerable<TestPromptDto>>(testPrompts);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestPromptDto createTestPromptDto)
    {
        // Отримуємо модель
        var model = await _fluxModelService.GetByIdAsync(createTestPromptDto.FluxModelId);
        if (model == null)
        {
            return NotFound($"Model with ID {createTestPromptDto.FluxModelId} not found.");
        }

        var trainings = await _trainingService.GetByModelIdAsync(createTestPromptDto.FluxModelId);
        // Вибираємо останній доступний TrainingId
        var latestTraining = trainings.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
        if (latestTraining == null)
        {
            return NotFound($"No available training for model ID {createTestPromptDto.FluxModelId}.");
        }

        // Створюємо тест
        var entry = await _testPromptService.CreateAsync(createTestPromptDto.PromptId, latestTraining.Id, createTestPromptDto.Text);
        var result = _mapper.Map<TestPromptDto>(entry);

        return Ok(result);
    }


    [HttpGet("run/{id:int}")]
    public async Task<IActionResult> Run(int id)
    {
        try
        {
            var testPrompt = await _testPromptService.GetByIdAsync(id);

            if (testPrompt is null)
            {
                return NotFound($"TestPrompt with ID {id} not found.");
            }

            var fluxModel = await _fluxModelService.GetByTrainingIdAsync(testPrompt.TrainingId);
            if (fluxModel is null)
            {
                return NotFound($"FluxModel not found.");
            }

            if (testPrompt.Status == "prepared")
            {
                // Sending request to ReplicateService to generate image
                var imageRequest = GenerateImageRequestInput.GetImageRequest(testPrompt.Text);
                var result = await _replicateService.GenerateImageAsync(fluxModel.Version, imageRequest);

                if (result is not null && result.Output.Count > 0)
                {
                    var imagePath = result.Output[0];
                    // Updating task status and result
                    testPrompt.Status = "completed";
                    testPrompt.ProcessedImageUrl = imagePath;
                    testPrompt.ImageUrl = await _replicateService.DownloadAndSaveImageAsync(imagePath);

                    await _testPromptService.UpdateResultAsync(testPrompt);
                }
            }

            var output = _mapper.Map<TestPromptDto>(testPrompt);
            return Ok(output);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error while running Job: {ex.Message}");
        }
    }
             
}
