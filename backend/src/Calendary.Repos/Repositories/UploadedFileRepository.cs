using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IUploadedFileRepository : IRepository<UploadedFile>
{
    Task<IEnumerable<UploadedFile>> GetByUserIdAsync(int userId, bool includeDeleted = false);
    Task<UploadedFile?> GetByIdAndUserIdAsync(int id, int userId);
    Task SoftDeleteAsync(int id, int userId);
}

public class UploadedFileRepository : IUploadedFileRepository
{
    private readonly ICalendaryDbContext _context;

    public UploadedFileRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UploadedFile>> GetAllAsync()
    {
        return await _context.UploadedFiles
            .Where(f => !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<UploadedFile?> GetByIdAsync(int id)
    {
        return await _context.UploadedFiles
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
    }

    public async Task<IEnumerable<UploadedFile>> GetByUserIdAsync(int userId, bool includeDeleted = false)
    {
        var query = _context.UploadedFiles
            .Where(f => f.UserId == userId);

        if (!includeDeleted)
        {
            query = query.Where(f => !f.IsDeleted);
        }

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<UploadedFile?> GetByIdAndUserIdAsync(int id, int userId)
    {
        return await _context.UploadedFiles
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && !f.IsDeleted);
    }

    public async Task AddAsync(UploadedFile entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.UploadedFiles.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UploadedFile entity)
    {
        _context.UploadedFiles.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.UploadedFiles.FindAsync(id);
        if (entity is not null)
        {
            _context.UploadedFiles.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int id, int userId)
    {
        var entity = await GetByIdAndUserIdAsync(id, userId);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            _context.UploadedFiles.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
