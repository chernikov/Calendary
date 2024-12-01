using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Senders;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Calendary.Model;
using Calendary.Model.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Calendary.Api.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/job")]
public class JobController  : BaseUserController
{
    private readonly IJobService _jobService;
    private readonly IJobTaskService _jobTaskService;
    private readonly IReplicateService _replicateService;
    private readonly IFluxModelService _fluxModelService;
    private readonly IRabbitMqSender _rabbitMqSender;
    private readonly IMapper _mapper;

    public JobController(IUserService userService, 
        IJobService jobService,
        IJobTaskService jobTaskService,
        IReplicateService replicateService,
        IFluxModelService fluxModelService,
        IRabbitMqSender rabbitMqSender,
        IMapper mapper) : base(userService)
    {
        _jobService = jobService;
        _jobTaskService = jobTaskService;
        _replicateService = replicateService;
        _fluxModelService = fluxModelService;
        _rabbitMqSender = rabbitMqSender;
        _mapper = mapper;
    }


    /// <summary>
    /// Генерація Job
    /// </summary>
    /// <param name="id">ID FluxModel</param>
    [HttpGet("default/{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            // Створюємо Default Job
            var job = await _jobService.CreateDefaultJobAsync(id);

            // Мапимо Job на JobDto
            var jobDto = _mapper.Map<JobDto>(job);

            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error while creating Default Job: {ex.Message}");
        }
    }

    /// <summary>
    /// Генерація Job
    /// </summary>
    /// <param name="id">ID FluxModel</param>
    [HttpGet("generate/{id:int}")]
    public async Task<IActionResult> Get(int id, [FromQuery] int promptThemeId)
    {
        try
        {
            // Створюємо Default Job
            var job = await _jobService.CreateJobAsync(id, promptThemeId);

            // Мапимо Job на JobDto
            var jobDto = _mapper.Map<JobDto>(job);

            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error while creating Default Job: {ex.Message}");
        }
    }

    [HttpGet("run/{id:int}")]
    public async Task<IActionResult> Run(int id)
    {
        try
        {
            // Завантаження Job та JobTasks
            var job = await _jobService.GetJobWithTasksAsync(id);
            
            if (job is null)
            {
                return NotFound($"Job with ID {id} not found.");
            }

            var fluxModel = await _fluxModelService.GetByIdAsync(job.FluxModelId);
            if (fluxModel is null)
            {
                return NotFound($"FluxModel with ID {job.FluxModelId} not found.");
            }

            // Виконання кожного JobTask
            foreach (var task in job.Tasks)
            {
                if (task.Status == "Pending")
                {
                    // Відправка запиту до ReplicateService для генерації зображення
                    var result = await _replicateService.GenerateImageAsync(fluxModel.Version, GetImageRequest(task.Prompt));

                    if (result is not null && result.Output.Count > 0)
                    {
                        var imagePath = result.Output[0];
                        // Оновлення статусу завдання та результату
                        task.Status = "Completed";
                        task.ProcessedImageUrl = imagePath;
                        task.ImageUrl = await _replicateService.DownloadAndSaveImageAsync(imagePath);

                        await _jobTaskService.UpdateResultAsync(task);
                    }
                }
            }
            var dbJob = await _jobService.GetJobWithTasksAsync(job.Id);

            if (dbJob is not null)
            {
                if (dbJob.Tasks.All(p => p.Status == "Completed"))
                {
                    await _jobService.UpdateStatusAsync(job.Id, "Completed");
                }
            }
            
            fluxModel.Status = "Completed";
            await _fluxModelService.UpdateStatusAsync(fluxModel);

            // Збереження змін у базі
            return Ok(new { message = $"Job with ID {id} executed successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error while running Job: {ex.Message}");
        }
    }


    [HttpGet("message/{id:int}")]
    public async Task<IActionResult> Message(int id)
    {
        try
        {
            // Завантаження Job та JobTasks
            var job = await _jobService.GetJobWithTasksAsync(id);

            if (job is null)
            {
                return NotFound($"Job with ID {id} not found.");
            }

            var fluxModel = await _fluxModelService.GetByIdAsync(job.FluxModelId);
            if (fluxModel is null)
            {
                return NotFound($"FluxModel with ID {job.FluxModelId} not found.");
            }

            foreach (var task in job.Tasks)
            {
                if (task.Status == "Pending")
                {
                    var taskDto = _mapper.Map<JobTaskMessage>(task);
                    var jsonTask = JsonSerializer.Serialize(taskDto);
                    await _rabbitMqSender.SendMessageAsync("create-prediction", jsonTask);
                }
            }
            return Ok(new { message = $"Job with ID {id} send message successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error while running Job: {ex.Message}");
        }
    }

    private GenerateImageInput GetImageRequest(Prompt prompt)
    {
        return new()
        {
            Prompt = prompt.Text,
            Model = "dev",
            LoraScale  = 1m,
            NumOutputs = 1,
            AspectRatio = "3:4",
            OutputFormat = "jpg",
            GuidanceScale = 3.5,
            OutputQuality = 90,
            PromptStrength = 0.8,
            ExtraLoraScale = 1m,
            NumInferenceSteps = 28
        };
    }


  
}
