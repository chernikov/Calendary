using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IPromptRepository : IRepository<Prompt>
{
    Task<IEnumerable<Prompt>> GetFullAllAsync(int? themeId, int? ageGender);

    Task AssignSeedAsync(int promptId, int seed);

    Task DeassignSeedAsync(int promptId, int seed);

    Task ClearSeedsAsync(int promptId);
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

    public async Task<IEnumerable<Prompt>> GetFullAllAsync(int? themeId, int? ageGender)
    {
        return await _context.Prompts
            .Include(p => p.Theme)
            .Include(p => p.Seeds)
            .Where(p => (!themeId.HasValue || p.ThemeId == themeId.Value) &&
             (!ageGender.HasValue || (int)p.AgeGender == ageGender.Value)).ToListAsync();
    }

    public async Task<Prompt?> GetByIdAsync(int id)
    {
        return await _context.Prompts
            .Include(p => p.Seeds)
            .FirstOrDefaultAsync(p => p.Id == id);
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


    public async Task AssignSeedAsync(int promptId, int seed)
    {
        var promptSeed = new PromptSeed
        {
            PromptId = promptId,
            Seed = seed
        };
        await _context.PromptSeeds.AddAsync(promptSeed);
        await _context.SaveChangesAsync();
    }

    public async Task DeassignSeedAsync(int promptId, int seed)
    {
        var promptSeed = await _context.PromptSeeds
            .FirstOrDefaultAsync(ps => ps.PromptId == promptId && ps.Seed == seed);

        if (promptSeed is not null)
        {
            _context.PromptSeeds.Remove(promptSeed);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearSeedsAsync(int promptId)
    {
        var listed = await _context.PromptSeeds
            .Where(p => p.PromptId == promptId).ToListAsync();
        _context.PromptSeeds.RemoveRange(listed);
        await _context.SaveChangesAsync();
    }
}