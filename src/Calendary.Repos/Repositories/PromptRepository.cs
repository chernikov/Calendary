using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IPromptRepository : IRepository<Prompt>
{
    
    Task<IEnumerable<Prompt>> GetFullAllAsync();

    Task<IEnumerable<Prompt>> GetFullByThemeIdAsync(int themeId);
}

public class PromptRepository : IPromptRepository
{
    private readonly ICalendaryDbContext _context;

    public PromptRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Prompt>> GetAllAsync()
    {
        return await _context.Prompts.ToListAsync();
    }

    public async Task<IEnumerable<Prompt>> GetFullAllAsync()
    {
        return await _context.Prompts
            .Include(p => p.Theme).ToListAsync();
    }

    public async Task<IEnumerable<Prompt>> GetFullByThemeIdAsync(int themeId)
    {
        return await _context.Prompts
            .Include(p => p.Theme)
            .Where(p => p.ThemeId == themeId).ToListAsync();
    }

    public async Task<Prompt?> GetByIdAsync(int id)
    {
        return await _context.Prompts.FindAsync(id);
    }

    public async Task AddAsync(Prompt entity)
    {
        await _context.Prompts.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Prompt entity)
    {
        _context.Prompts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Prompts.FindAsync(id);
        if (entity is not null)
        {
            _context.Prompts.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

  
}