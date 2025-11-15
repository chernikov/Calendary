using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IFluxModelRepository : IRepository<FluxModel>
{
    Task<IReadOnlyCollection<FluxModel>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<FluxModel>> GetByCategoryIdAsync(int categoryId);
    Task<IList<FluxModel>> GetListByUserIdAsync(int userId);
    Task<FluxModel?> GetCurrentByUserIdAsync(int useId);
    Task<FluxModel?> GetFullAsync(int id);
    Task<FluxModel?> GetUserFluxModelAsync(int userId, int fluxModelId);
    Task<FluxModel?> GetFullByIdAsync(int id);
    Task<bool> IsNameUniqueForUserAsync(int userId, string name, int? excludeId = null);
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
            .Include(p => p.Category)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<FluxModel?> GetByIdAsync(int id)
    {
        return await _context.FluxModels.FindAsync(id);
    }


    public async Task<FluxModel?> GetFullByIdAsync(int id)
    {
        return await _context.FluxModels
            .Include(p => p.Category)
            .Include(p => p.Trainings)
            .Include(p => p.Jobs)
                .ThenInclude(p => p.Theme)
            .Include(p => p.Jobs)
                .ThenInclude(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
            
            
    }

    public async Task<FluxModel?> GetCurrentByUserIdAsync(int userId)
    {
        return await _context.FluxModels
            .Include(p => p.Category)
            .Include(p => p.Trainings)
            .Include(p => p.Jobs)
                .ThenInclude(p => p.Theme)
            .Include(p => p.Jobs)
                .ThenInclude(p => p.Tasks)
            .Where(fm => fm.UserId == userId && !fm.IsArchive)
            .OrderByDescending(fm => fm.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<IList<FluxModel>> GetListByUserIdAsync(int userId)
    {
        return await _context.FluxModels
            .Include(fm => fm.Trainings.Where(t => !t.IsDeleted))
            .Where(f => f.UserId == userId)
            .ToListAsync();
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
            .Include(p => p.Category)
            .Include(p => p.Photos)
            .Include(p => p.Trainings)
            .Include(p => p.Jobs)
                .ThenInclude(p => p.Tasks)
            .FirstOrDefaultAsync(fm => fm.Id == id);
    }

    public async Task<IEnumerable<FluxModel>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.FluxModels
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(fm => fm.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<FluxModel?> GetUserFluxModelAsync(int userId, int fluxModelId)
    {
        return await _context.FluxModels.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == fluxModelId);
    }

    public async Task<bool> IsNameUniqueForUserAsync(int userId, string name, int? excludeId = null)
    {
        var query = _context.FluxModels.Where(fm => fm.UserId == userId && fm.Name == name && !fm.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(fm => fm.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}