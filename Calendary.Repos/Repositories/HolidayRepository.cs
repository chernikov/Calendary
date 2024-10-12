using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public class HolidayRepository : IRepository<Holiday>
{
    private readonly ICalendaryDbContext _context;

    public HolidayRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Holiday>> GetAllAsync()
    {
        return await _context.Holidays.ToListAsync();
    }

    public async Task<Holiday?> GetByIdAsync(int id)
    {
        return await _context.Holidays.FindAsync(id);
    }

    public async Task AddAsync(Holiday entity)
    {
        await _context.Holidays.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Holiday entity)
    {
        _context.Holidays.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Holidays.FindAsync(id);
        if (entity is not null)
        {
            _context.Holidays.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}