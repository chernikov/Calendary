using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IPhotoRepository : IRepository<Photo>
{
}

public class PhotoRepository : IPhotoRepository
{
    private readonly ICalendaryDbContext _context;

    public PhotoRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Photo>> GetAllAsync()
    {
        return await _context.Photos.ToListAsync();
    }

    public async Task<Photo?> GetByIdAsync(int id)
    {
        return await _context.Photos.FindAsync(id);
    }

    public async Task AddAsync(Photo entity)
    {
        await _context.Photos.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Photo entity)
    {
        _context.Photos.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Photos.FindAsync(id);
        if (entity is not null)
        {
            _context.Photos.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}