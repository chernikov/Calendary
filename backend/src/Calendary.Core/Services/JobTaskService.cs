using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IJobTaskService 
{
    Task<JobTask?> GetByIdAsync(int jobTaskId);

    Task<JobTask?> GetByIdWithPromptAsync(int jobTaskId);
    Task UpdateResultAsync(JobTask task);
    Task DeleteAsync(int id);
}

public  class JobTaskService : IJobTaskService
{
    private readonly IJobTaskRepository _jobTaskRepository;

    public JobTaskService(IJobTaskRepository jobTaskRepository)
    {
        _jobTaskRepository = jobTaskRepository;
    }

    public Task<JobTask?> GetByIdAsync(int jobTaskId)
        => _jobTaskRepository.GetByIdAsync(jobTaskId);

    public Task<JobTask?> GetByIdWithPromptAsync(int jobTaskId)
        => _jobTaskRepository.GetByIdWithPromptAsync(jobTaskId);

    public async Task UpdateResultAsync(JobTask task)
    {
        var entry = await _jobTaskRepository.GetByIdAsync(task.Id);
        if (entry is null)
        {
            return;
        }
        entry.ProcessedImageUrl = task.ProcessedImageUrl;
        entry.ImageUrl = task.ImageUrl;
        entry.Status = task.Status;
        await _jobTaskRepository.UpdateAsync(entry);
    }

    public async Task DeleteAsync(int id)
    {
        await _jobTaskRepository.DeleteAsync(id);
    }
}
