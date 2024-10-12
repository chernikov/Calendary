using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public class CalendarRepository : IRepository<Calendar>
{
    private readonly ICalendaryDbContext _context;

    public CalendarRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Calendar>> GetAllAsync()
    {
        return await _context.Calendars.ToListAsync();
    }

    public async Task<Calendar?> GetByIdAsync(int id)
    {
        return await _context.Calendars.FindAsync(id);
    }

    public async Task AddAsync(Calendar entity)
    {
        await _context.Calendars.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Calendar entity)
    {
        _context.Calendars.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Calendars.FindAsync(id);
        if (entity is not null)
        {
            _context.Calendars.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}