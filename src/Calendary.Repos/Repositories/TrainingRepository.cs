using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface ITrainingRepository : IRepository<Training>
{
    Task<IEnumerable<Training>> GetByModelIdAsync(int modelId);
    Task<Training?> GetByReplicateIdAsync(string replicateId);
}

public class TrainingRepository : ITrainingRepository
{
    private readonly ICalendaryDbContext _context;

    public TrainingRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Training>> GetAllAsync()
    {
        return await _context.Trainings.ToListAsync();
    }

    public async Task<Training?> GetByIdAsync(int id)
    {
        return await _context.Trainings.FindAsync(id);
    }

    public async Task AddAsync(Training entity)
    {
        await _context.Trainings.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Training entity)
    {
        _context.Trainings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Trainings.FindAsync(id);
        if (entity is not null)
        {
            _context.Trainings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public Task<Training?> GetByReplicateIdAsync(string replicateId)
        => _context.Trainings.FirstOrDefaultAsync(t => t.ReplicateId == replicateId);

    public async Task<IEnumerable<Training>> GetByModelIdAsync(int modelId)
    {
        return await _context.Trainings.Where(t => t.FluxModelId == modelId).ToListAsync();
    }
}
