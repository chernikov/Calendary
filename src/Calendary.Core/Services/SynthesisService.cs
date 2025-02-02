using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface ISynthesisService
{
    Task<IEnumerable<Synthesis>> GetByPromptIdAsync(int idPrompt);

    Task<Synthesis> CreateAsync(int promptId, int trainingId, string? text, int? seed);

    Task<IEnumerable<Synthesis>> GetFullAllAsync();

    Task<Synthesis?> GetByIdAsync(int id);
    
    Task DeleteAsync(int id);
    Task UpdateResultAsync(Synthesis synthesis);
}

public class SynthesisService : ISynthesisService
{

    private readonly ISynthesisRepository _synthesisRepository;
    private readonly IPromptRepository _promptRepository;
    private readonly ITrainingRepository _trainingRepository;

    public SynthesisService(ISynthesisRepository synthesisRepository, 
            IPromptRepository promptRepository, 
            ITrainingRepository trainingRepository)
    {
        _synthesisRepository = synthesisRepository;
        _promptRepository = promptRepository;
        _trainingRepository = trainingRepository;
    }

    public Task<IEnumerable<Synthesis>> GetByPromptIdAsync(int idPrompt)
     => _synthesisRepository.GetByPromptIdAsync(idPrompt);


    public async Task<Synthesis> CreateAsync(int promptId, int trainingId, string? text, int? seed)
    {
        var prompt = await _promptRepository.GetByIdAsync(promptId);
        var training = await _trainingRepository.GetByIdAsync(trainingId);

        if (prompt == null || training == null)
        {
            throw new ArgumentException("Invalid promptId or trainingId");
        }

        var synthesis = new Synthesis
        {
            PromptId = promptId,
            TrainingId = trainingId,
            Text = text ?? prompt.Text,
            Seed = seed,
            Status = "prepared",
            CreatedAt = DateTime.UtcNow,
            Prompt = prompt,
            Training = training
        };

        await _synthesisRepository.AddAsync(synthesis);
        return synthesis;
    }

    public Task<IEnumerable<Synthesis>> GetFullAllAsync()
        => _synthesisRepository.GetAllAsync();

    public async Task<Synthesis?> GetByIdAsync(int id)
    {
        return await _synthesisRepository.GetByIdAsync(id);
    }
   
    public async Task UpdateResultAsync(Synthesis synthesis)
    {
        var entity = await _synthesisRepository.GetByIdAsync(synthesis.Id);
        if (entity is null)
        {
            return;
        }
        entity.ReplicateId = synthesis.ReplicateId;   
        entity.OutputSeed = synthesis.OutputSeed;  
        entity.ProcessedImageUrl = synthesis.ProcessedImageUrl;
        entity.ImageUrl = synthesis.ImageUrl;
        entity.Status = synthesis.Status;
        await _synthesisRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _synthesisRepository.DeleteAsync(id);
    }
}
