using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IJobService
{
    Task<Job> CreateDefaultJobAsync(int fluxModelId);
    Task<Job> CreateJobAsync(int fluxModelId, int promptThemeId);
    Task<Job?> GetJobWithTasksAsync(int id);
    Task UpdateStatusAsync(int id, string status);
}

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobTaskRepository _jobTaskRepository;
    private readonly IFluxModelRepository _fluxModelRepository;
    private readonly IPromptThemeRepository _promptThemeRepository;
    private readonly IPromptRepository _promptRepository;

    public JobService(IJobRepository jobRepository,
            IJobTaskRepository jobTaskRepository,
            IFluxModelRepository fluxModelRepository,
            IPromptThemeRepository promptThemeRepository,
            IPromptRepository promptRepository)
    {
        _fluxModelRepository = fluxModelRepository;
        _promptThemeRepository = promptThemeRepository;
        _promptRepository = promptRepository;
        _jobRepository = jobRepository;
        _jobTaskRepository = jobTaskRepository;
    }

    public async Task<Job> CreateDefaultJobAsync(int fluxModelId)
    {
        const string defaultPromptTheme = "default";
        var promptTheme = await _promptThemeRepository.GetByNameAsync(defaultPromptTheme);

        if (promptTheme is null)
        {
            throw new Exception($"PromptTheme with name {defaultPromptTheme} not found.");
        }

        return await CreateJobInternalAsync(fluxModelId, promptTheme.Id);
    }

    public async Task<Job> CreateJobAsync(int fluxModelId, int promptThemeId)
    {
        return await CreateJobInternalAsync(fluxModelId, promptThemeId);
    }

    private async Task<Job> CreateJobInternalAsync(int fluxModelId, int promptThemeId)
    {
        // Завантажуємо FluxModel для перевірки
        var fluxModel = await _fluxModelRepository.GetByIdAsync(fluxModelId);
        if (fluxModel is null)
        {
            throw new Exception($"FluxModel with ID {fluxModelId} not found.");
        }

        // Визначаємо тему і гендер
        var ageGender = fluxModel.AgeGender;
        var promptTheme = await _promptThemeRepository.GetByIdAsync(promptThemeId);

        if (promptTheme is null)
        {
            throw new Exception($"PromptTheme with Id {promptThemeId} not found.");
        }

        // Створюємо новий Job
        var job = new Job
        {
            UserId = fluxModel.UserId,
            FluxModelId = fluxModelId,
            ThemeId = promptTheme.Id,
            CreatedAt = DateTime.UtcNow,
            Status = "pending",
        };

        await _jobRepository.AddAsync(job);

        var themesPrompts = await _promptRepository.GetFullByThemeIdAsync(promptTheme.Id);
        var genderPrompts = themesPrompts.Where(p => p.AgeGender == ageGender);

        // Створюємо JobTasks на основі prompt
        var jobTasks = genderPrompts.Select(prompt => new JobTask
        {
            JobId = job.Id,
            PromptId = prompt.Id,
            FluxModelId = fluxModelId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _jobTaskRepository.AddRangeAsync(jobTasks);
        return job;
    }

    public Task<Job?> GetJobWithTasksAsync(int id)
        => _jobRepository.GetJobWithTasksAsync(id);

    public async Task UpdateStatusAsync(int id, string status)
    {
        var entry = await _jobRepository.GetByIdAsync(id);
        if (entry is not null)
        {
            entry.Status = status;
            await _jobRepository.UpdateAsync(entry);
        }
    }
}
