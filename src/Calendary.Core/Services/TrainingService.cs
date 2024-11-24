using Calendary.Core.Services.Models;
using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface ITrainingService
{
    Task<Training> SaveAsync(int fluxModelId, TrainModelResponse response);
    Task<Training?> GetByIdAsync(int id);

    Task<Training?> GetByReplicateIdAsync(string replicateId);
    Task<IEnumerable<Training>> GetAllAsync();
    Task UpdateStatusAsync(int trainingId, string status);
}

public class TrainingService : ITrainingService
{
    private readonly ITrainingRepository _trainingRepository;
    
    public TrainingService(ITrainingRepository trainingRepository)
    {
        _trainingRepository = trainingRepository;
    }

    /// <summary>
    /// Зберігає інформацію про тренування.
    /// </summary>
    /// <param name="fluxModelId">ID моделі</param>
    /// <param name="response">Дані відповіді від Replicate</param>
    /// <returns>Збережений об'єкт Training</returns>
    public async Task<Training> SaveAsync(int fluxModelId, TrainModelResponse response)
    {
        var training = new Training
        {
            FluxModelId = fluxModelId,
            ReplicateId = response.Id,
            Status = response.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _trainingRepository.AddAsync(training);

        return training;
    }

    /// <summary>
    /// Отримати тренування за ID.
    /// </summary>
    /// <param name="id">ID тренування</param>
    /// <returns>Об'єкт Training або null</returns>
    public async Task<Training?> GetByIdAsync(int id)
    {
        return await _trainingRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Отримати всі тренування.
    /// </summary>
    /// <returns>Список тренувань</returns>
    public async Task<IEnumerable<Training>> GetAllAsync()
    {
        return await _trainingRepository.GetAllAsync();
    }

    /// <summary>
    /// Оновлює статус тренування.
    /// </summary>
    /// <param name="trainingId">ID тренування</param>
    /// <param name="status">Новий статус</param>
    public async Task UpdateStatusAsync(int trainingId, string status)
    {
        var training = await _trainingRepository.GetByIdAsync(trainingId);
        if (training == null)
        {
            throw new InvalidOperationException($"Training with ID {trainingId} not found.");
        }

        training.Status = status;
        if (status == "succeeded")
        {
            training.CompletedAt = DateTime.UtcNow;
        }
        await _trainingRepository.UpdateAsync(training);
       
    }

    public Task<Training?> GetByReplicateIdAsync(string replicateId)
        => _trainingRepository.GetByReplicateIdAsync(replicateId);
    
}