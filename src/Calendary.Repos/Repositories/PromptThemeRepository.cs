using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IPromptThemeRepository : IRepository<PromptTheme>
{
    Task<IEnumerable<PromptTheme>> GetAllPublishedAsync();
    Task<PromptTheme?> GetByNameAsync(string name);
}

public class PromptThemeRepository : IPromptThemeRepository
{
    private readonly ICalendaryDbContext _context;

    public PromptThemeRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PromptTheme>> GetAllAsync()
    {
        return await _context.PromptThemes.ToListAsync();
    }


    public async Task<IEnumerable<PromptTheme>> GetAllPublishedAsync()
    {
        return await _context.PromptThemes.
            Where(p => p.IsPublished).ToListAsync();
    }
    public async Task<PromptTheme?> GetByIdAsync(int id)
    {
        return await _context.PromptThemes.FindAsync(id);
    }

    public async Task AddAsync(PromptTheme entity)
    {
        await _context.PromptThemes.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

  

    public async Task UpdateAsync(PromptTheme entity)
    {
        _context.PromptThemes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.PromptThemes.FindAsync(id);
        if (entity is not null)
        {
            _context.PromptThemes.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public Task<PromptTheme?> GetByNameAsync(string name)
        => _context.PromptThemes.FirstOrDefaultAsync(p => p.Name == name);

  
}