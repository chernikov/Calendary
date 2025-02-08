using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;


public interface ISynthesisRepository : IRepository<Synthesis>
{
    Task<IEnumerable<Synthesis>> GetByTrainingIdAsync(int fluxModelId);
    Task<IEnumerable<Synthesis>> GetByPromptIdAsync(int idPrompt);
}

public class SynthesisRepository : ISynthesisRepository
{
    private readonly ICalendaryDbContext _context;

    public SynthesisRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Synthesis entity)
    {
        await _context.Synthesises.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Synthesises.FindAsync(id);
        if (entity != null)
        {
            _context.Synthesises.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Synthesis>> GetAllAsync()
    {
        return await _context.Synthesises.ToListAsync();
    }

    public async Task<IEnumerable<Synthesis>> GetByTrainingIdAsync(int trainingId)
    {
        return await _context.Synthesises
                .Include(p => p.Prompt)
                .Where(p => p.TrainingId == trainingId)
                .ToListAsync();
    }

    public async Task<Synthesis?> GetByIdAsync(int id)
    {
        return await _context.Synthesises.FindAsync(id);
    }

    public async Task<IEnumerable<Synthesis>> GetByPromptIdAsync(int idPrompt)
    {
        return await _context.Synthesises.Where(p => p.PromptId == idPrompt).ToListAsync();
    }

    public async Task UpdateAsync(Synthesis entity)
    {
        _context.Synthesises.Update(entity);
        await _context.SaveChangesAsync();
    }
}
