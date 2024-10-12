using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public class CountryRepository : IRepository<Country>
{
    private readonly ICalendaryDbContext _context;

    public CountryRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Country>> GetAllAsync()
    {
        return await _context.Countries.ToListAsync();
    }

    public async Task<Country?> GetByIdAsync(int id)
    {
        return await _context.Countries.FindAsync(id);
    }

    public async Task AddAsync(Country entity)
    {
        await _context.Countries.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Country entity)
    {
        _context.Countries.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Countries.FindAsync(id);
        if (entity is not null)
        {
            _context.Countries.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
