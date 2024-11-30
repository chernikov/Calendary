using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface ITestPromptService
{
    Task<IEnumerable<TestPrompt>> GetByPromptIdAsync(int idPrompt);

    Task<TestPrompt> CreateAsync(int promptId, int trainingId, string? text);

    Task<IEnumerable<TestPrompt>> GetFullAllAsync();

    Task<TestPrompt?> GetByIdAsync(int id);
    
    Task DeleteAsync(int id);
    Task UpdateResultAsync(TestPrompt testPrompt);
}

public class TestPromptService : ITestPromptService
{

    private readonly ITestPromptRepository _testPromptRepository;
    private readonly IPromptRepository _promptRepository;
    private readonly ITrainingRepository _trainingRepository;

    public TestPromptService(ITestPromptRepository testPromptRepository, 
            IPromptRepository promptRepository, 
            ITrainingRepository trainingRepository)
    {
        _testPromptRepository = testPromptRepository;
        _promptRepository = promptRepository;
        _trainingRepository = trainingRepository;
    }

    public Task<IEnumerable<TestPrompt>> GetByPromptIdAsync(int idPrompt)
     => _testPromptRepository.GetByPromptIdAsync(idPrompt);


    public async Task<TestPrompt> CreateAsync(int promptId, int trainingId, string? text)
    {
        var prompt = await _promptRepository.GetByIdAsync(promptId);
        var training = await _trainingRepository.GetByIdAsync(trainingId);

        if (prompt == null || training == null)
        {
            throw new ArgumentException("Invalid promptId or trainingId");
        }

        var testPrompt = new TestPrompt
        {
            PromptId = promptId,
            TrainingId = trainingId,
            Text = text ?? prompt.Text,
            Status = "prepared",
            CreatedAt = DateTime.UtcNow,
            Prompt = prompt,
            Training = training
        };

        await _testPromptRepository.AddAsync(testPrompt);
        return testPrompt;
    }

    public Task<IEnumerable<TestPrompt>> GetFullAllAsync()
        => _testPromptRepository.GetAllAsync();

    public async Task<TestPrompt?> GetByIdAsync(int id)
    {
        return await _testPromptRepository.GetByIdAsync(id);
    }
   
    public async Task UpdateResultAsync(TestPrompt testPrompt)
    {
        var entity = await _testPromptRepository.GetByIdAsync(testPrompt.Id);
        if (entity is null)
        {
            return;
        }
        entity.ProcessedImageUrl = testPrompt.ProcessedImageUrl;
        entity.ImageUrl = testPrompt.ImageUrl;
        entity.Status = testPrompt.Status;
        await _testPromptRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _testPromptRepository.DeleteAsync(id);
    }
}
