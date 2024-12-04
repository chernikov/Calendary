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
public class JobController : BaseUserController
{
    private readonly IJobService _jobService;
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
            // Створюємо Job
            var job = await _jobService.CreateJobAsync(id, promptThemeId);

            var fluxModel = await _fluxModelService.GetByIdAsync(job.FluxModelId);
            if (fluxModel is null)
            {
                throw new Exception($"FluxModel with ID {job.FluxModelId} not found.");
            }
            var jobWithTasks = await _jobService.GetJobWithTasksAsync(job.Id);
            if (jobWithTasks is null)
            {
                throw new Exception($"job with ID {job.Id} not found.");
            }
            foreach (var task in jobWithTasks.Tasks)
            {
                if (task.Status == "pending")
                {
                    var taskDto = _mapper.Map<JobTaskMessage>(task);
                    var jsonTask = JsonSerializer.Serialize(taskDto);
                    await _rabbitMqSender.SendMessageAsync("create-prediction", jsonTask);
                }
            }

            fluxModel.Status = "image_generating";
            await _fluxModelService.UpdateStatusAsync(fluxModel);

            // Мапимо Job на JobDto
            var jobDto = _mapper.Map<JobDto>(job);

            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error while creating Default Job: {ex.Message}");
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
                if (task.Status == "pending")
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
}
