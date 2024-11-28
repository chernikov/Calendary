using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IFluxModelRepository : IRepository<FluxModel>
{
    Task<IReadOnlyCollection<FluxModel>> GetAllAsync(int page, int pageSize);
    Task<FluxModel?> GetCurrentByUserIdAsync(int useId);
    Task<FluxModel?> GetFullAsync(int id);
}

public class FluxModelRepository : IFluxModelRepository
{
    private readonly ICalendaryDbContext _context;

    public FluxModelRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

  

    public async Task<IEnumerable<FluxModel>> GetAllAsync()
    {
        return await _context.FluxModels.ToListAsync();
    }

    public async Task<IReadOnlyCollection<FluxModel>> GetAllAsync(int page, int pageSize)
    {
        return await _context.FluxModels
            .Include(fm => fm.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<FluxModel?> GetByIdAsync(int id)
    {
        return await _context.FluxModels.FindAsync(id);
    }

    public async Task<FluxModel?> GetCurrentByUserIdAsync(int userId)
    {
        return await _context.FluxModels
            .Include(p => p.Trainings)
            .Include(p => p.Jobs)
                .ThenInclude(p => p.Tasks)
            .Where(fm => fm.UserId == userId && !fm.IsArchive)
            .OrderByDescending(fm => fm.Id)
            .FirstOrDefaultAsync();
    }


    public async Task AddAsync(FluxModel entity)
    {
        await _context.FluxModels.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FluxModel entity)
    {
        _context.FluxModels.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.FluxModels.FindAsync(id);
        if (entity is not null)
        {
            _context.FluxModels.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public Task<FluxModel?> GetFullAsync(int id)
    {
        return _context.FluxModels
            .Include(p => p.User)
            .Include(p => p.Photos)
            .Include(p => p.Trainings)
            .Include(p => p.Jobs)
                .ThenInclude(p => p.Tasks)
            .FirstOrDefaultAsync(fm => fm.Id == id);
    }
}