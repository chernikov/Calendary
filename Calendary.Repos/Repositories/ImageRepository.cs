using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IImageRepository : IRepository<Image>
{

}
public class ImageRepository : IImageRepository
{
    private readonly ICalendaryDbContext _context;

    public ImageRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Image>> GetAllAsync()
    {
        return await _context.Images.ToListAsync();
    }

    public async Task<Image?> GetByIdAsync(int id)
    {
        return await _context.Images.FindAsync(id);
    }

    public async Task AddAsync(Image entity)
    {
        await _context.Images.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Image entity)
    {
        _context.Images.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Images.FindAsync(id);
        if (entity is not null)
        {
            _context.Images.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}