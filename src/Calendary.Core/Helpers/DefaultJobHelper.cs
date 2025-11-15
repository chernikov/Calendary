using Calendary.Core.Services;
using Calendary.Core.Services.Models;

namespace Calendary.Core.Helpers;


public interface IDefaultJobHelper
{
    Task RunAsync(int fluxModelId);
}

internal class DefaultJobHelper : IDefaultJobHelper
{
    private readonly IFluxModelService fluxModelService;
    private readonly IJobService jobService;
    private readonly IJobTaskService jobTaskService;
    private readonly IReplicateService replicateService;

    public DefaultJobHelper(IFluxModelService fluxModelService, 
            IJobService jobService,
            IJobTaskService jobTaskService,
            IReplicateService replicateService)
    {
        this.fluxModelService = fluxModelService;
        this.jobService = jobService;
        this.jobTaskService = jobTaskService;
        this.replicateService = replicateService;
    }

    public async Task RunAsync(int fluxModelId)
    {

        var fluxModel = await fluxModelService.GetFullAsync(fluxModelId); 
        if (fluxModel is null)
        {
            return;
        }
        var job = await jobService.CreateDefaultJobAsync(fluxModelId);
        var jobWithTasks = await jobService.GetJobWithTasksAsync(job.Id);

        if (jobWithTasks is null)
        {
            return;
        }

        foreach(var task in jobWithTasks.Tasks)
        {
            var imageRequest = GenerateImageInput.GetImageRequest(task.Prompt.Text, task.Seed);
            var predictionId = await replicateService.StartImageGenerationAsync(fluxModel.Version, imageRequest);
            var result = await replicateService.GenerateImageAsync(predictionId);

            if (result is not null && result.Output.Count > 0)
            {
                var imagePath = result.Output[0];
                // Updating task status and result
                var seed = result.ExtractSeedFromLogs();
                task.ReplicateId = result.Id;
                task.OutputSeed = seed;
                // Оновлення статусу завдання та результату
                task.Status = "completed";
                task.ProcessedImageUrl = imagePath;
                task.ImageUrl = await replicateService.DownloadAndSaveImageAsync(imagePath);

                await jobTaskService.UpdateResultAsync(task);
            }
        }
        await jobService.UpdateStatusAsync(job.Id, "completed");

        fluxModel.Status = "exampled";
        await fluxModelService.UpdateStatusAsync(fluxModel);

    }
}
