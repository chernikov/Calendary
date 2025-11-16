using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IUserPhotoRepository : IRepository<UserPhoto>
{
    Task<IReadOnlyCollection<UserPhoto>> GetByUserIdAsync(int userId, bool includeDeleted = false);
    Task<UserPhoto?> GetByIdAndUserIdAsync(int id, int userId);
    Task<int> GetCountByUserIdAsync(int userId);
    Task SoftDeleteAsync(int id, int userId);
}

public class UserPhotoRepository : IUserPhotoRepository
{
    private readonly ICalendaryDbContext _context;

    public UserPhotoRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserPhoto>> GetAllAsync()
    {
        return await _context.UserPhotos
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserPhoto?> GetByIdAsync(int id)
    {
        return await _context.UserPhotos
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<UserPhoto?> GetByIdAndUserIdAsync(int id, int userId)
    {
        return await _context.UserPhotos
            .Where(p => p.Id == id && p.UserId == userId && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<UserPhoto>> GetByUserIdAsync(int userId, bool includeDeleted = false)
    {
        var query = _context.UserPhotos.Where(p => p.UserId == userId);

        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(int userId)
    {
        return await _context.UserPhotos
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .CountAsync();
    }

    public async Task AddAsync(UserPhoto entity)
    {
        await _context.UserPhotos.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserPhoto entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.UserPhotos.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.UserPhotos.FindAsync(id);
        if (entity is not null)
        {
            _context.UserPhotos.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int id, int userId)
    {
        var entity = await GetByIdAndUserIdAsync(id, userId);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(entity);
        }
    }
}
