using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;
public interface IPromptService
{
    Task<IEnumerable<Prompt>> GetFullAllAsync(int? themeId, int? ageGender);
    Task<Prompt?> GetByIdAsync(int id);
    Task CreateAsync(Prompt prompt);
    Task UpdateAsync(Prompt prompt);
    Task DeleteAsync(int id);

    Task AssignSeedAsync(int promptId, int seed);

    Task DeassignSeedAsync(int promptId, int seed);

    Task ClearSeedsAsync(int promptId);
}

public class PromptService : IPromptService
{
    private readonly IPromptRepository _promptRepository;

    public PromptService(IPromptRepository promptRepository)
    {
        _promptRepository = promptRepository;
    }

    public Task<IEnumerable<Prompt>> GetFullAllAsync(int? themeId, int? ageGender)
        => _promptRepository.GetFullAllAsync(themeId, ageGender); 
   
    public async Task<Prompt?> GetByIdAsync(int id)
    {
        return await _promptRepository.GetByIdAsync(id);
    }

    public async Task CreateAsync(Prompt prompt)
    {
        prompt.Theme = null!;
        await _promptRepository.AddAsync(prompt);
    }

    public async Task UpdateAsync(Prompt prompt)
    {
        var entity = await _promptRepository.GetByIdAsync(prompt.Id);
        if (entity is null)
        {
            return;
        }
        entity.ThemeId = prompt.ThemeId;
        entity.AgeGender = prompt.AgeGender;

        if (entity.Text != prompt.Text)
        {
            await _promptRepository.ClearSeedsAsync(entity.Id);
        }
        entity.Text = prompt.Text;
        await _promptRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _promptRepository.DeleteAsync(id);
    }

    public Task AssignSeedAsync(int promptId, int seed)
        => _promptRepository.AssignSeedAsync(promptId, seed);
    
    public Task DeassignSeedAsync(int promptId, int seed)
        => _promptRepository.DeassignSeedAsync(promptId, seed);  
    
    public Task ClearSeedsAsync(int promptId)
        => _promptRepository.ClearSeedsAsync(promptId);
}