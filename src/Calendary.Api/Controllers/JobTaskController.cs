using AutoMapper;
using Calendary.Core.Services.Models;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Calendary.Model;
using Calendary.Api.Dtos;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("/api/job-task")]
public class JobTaskController : Controller
{
    private readonly IJobService _jobService;
    private readonly IJobTaskService _jobTaskService;
    private readonly IReplicateService _replicateService;
    private readonly IFluxModelService _fluxModelService;
    private readonly IMapper _mapper;

    public JobTaskController(
        IJobService jobService,
        IJobTaskService jobTaskService,
        IReplicateService replicateService,
        IFluxModelService fluxModelService,
        IMapper mapper)
    {
        _jobService = jobService;
        _jobTaskService = jobTaskService;
        _replicateService = replicateService;
        _fluxModelService = fluxModelService;
        _mapper = mapper;
    }

    [HttpGet("run/{id:int}")]
    public async Task<IActionResult> Run(int id)
    {
        try
        {
            var task = await _jobTaskService.GetByIdWithPromptAsync(id);

            if (task is null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            var fluxModel = await _fluxModelService.GetByIdAsync(task.FluxModelId);
            if (fluxModel is null)
            {
                return NotFound($"FluxModel with ID {task.FluxModelId} not found.");
            }


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
            var dbJob = await _jobService.GetJobWithTasksAsync(task.JobId);
            if (dbJob is not null)
            {
                if (dbJob.Tasks.All(p => p.Status == "Completed"))
                {
                    await _jobService.UpdateStatusAsync(dbJob.Id, "Completed");
                }
            }

            var output = _mapper.Map<JobTaskDto>(task);
            return Ok(output);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error while running Job: {ex.Message}");
        }
    }

    private GenerateImageRequestInput GetImageRequest(Prompt prompt)
    {
        return new()
        {
            Prompt = prompt.Text,
            Model = "dev",
            LoraScale = 1m,
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
