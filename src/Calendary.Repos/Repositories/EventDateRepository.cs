using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IEventDateRepository : IRepository<EventDate>
{
    Task<IEnumerable<EventDate>> GetAllByUserIdAsync(int userId);
}

public class EventDateRepository : IEventDateRepository
{
    private readonly ICalendaryDbContext _context;

    public EventDateRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventDate>> GetAllAsync()
    {
        return await _context.EventDates.ToListAsync();
    }


    public async Task<IEnumerable<EventDate>> GetAllByUserIdAsync(int userId)
    {
        var setting = _context.UserSettings.FirstOrDefault(p => p.UserId == userId);
        if (setting != null)
        {
            return await _context.EventDates.Where(p => p.UserSettingId == setting.Id).ToListAsync();
        }
        return Array.Empty<EventDate>();
    }

    public async Task<EventDate?> GetByIdAsync(int id)
    {
        return await _context.EventDates.FindAsync(id);
    }

    public async Task AddAsync(EventDate entity)
    {
        await _context.EventDates.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(EventDate entity)
    {
        _context.EventDates.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.EventDates.FindAsync(id);
        if (entity is not null)
        {
            _context.EventDates.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}