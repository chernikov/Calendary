using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IPromptThemeService
{
    Task<IEnumerable<PromptTheme>> GetAllAsync();
    Task CreateAsync(PromptTheme theme);
    Task UpdateAsync(PromptTheme theme);
    Task DeleteAsync(int id);
    Task<PromptTheme?> GetByIdAsync(int id);
}

public class PromptThemeService : IPromptThemeService
{
    private readonly IPromptThemeRepository _promptThemeRepository;


    public PromptThemeService(IPromptThemeRepository promptThemeRepository)
    {
        _promptThemeRepository = promptThemeRepository;
    }

    public async Task<IEnumerable<PromptTheme>> GetAllAsync()
    {
        return await _promptThemeRepository.GetAllAsync();
    }

    public Task<PromptTheme?> GetByIdAsync(int id)
        => _promptThemeRepository.GetByIdAsync(id); 
    
    public Task CreateAsync(PromptTheme theme)
        => _promptThemeRepository.AddAsync(theme);

    public async Task UpdateAsync(PromptTheme theme)
    {
        var entity = await _promptThemeRepository.GetByIdAsync(theme.Id);
        if (entity is null)
        {
            return;
        }
        entity.Name = theme.Name;
        await _promptThemeRepository.UpdateAsync(entity);
    }

    public  Task DeleteAsync(int id)
        => _promptThemeRepository.DeleteAsync(id);

   
}
