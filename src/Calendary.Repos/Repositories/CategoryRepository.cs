using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface ICategoryRepository : IRepository<Category>
{

}

public class CategoryRepository : ICategoryRepository
{
    private readonly ICalendaryDbContext _context;

    public CategoryRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task AddAsync(Category entity)
    {
        await _context.Categories.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category entity)
    {
        _context.Categories.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Categories.FindAsync(id);
        if (entity is not null)
        {
            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}