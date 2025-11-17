using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IUserCalendarRepository : IRepository<UserCalendar>
{
    Task<IEnumerable<UserCalendar>> GetByUserIdAsync(int userId, bool includeDeleted = false);
    Task<UserCalendar?> GetByIdAndUserIdAsync(int id, int userId);
    Task<bool> UserOwnsCalendarAsync(int calendarId, int userId);
    Task SoftDeleteAsync(int id, int userId);
}

public class UserCalendarRepository : IUserCalendarRepository
{
    private readonly ICalendaryDbContext _context;

    public UserCalendarRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserCalendar>> GetAllAsync()
    {
        return await _context.UserCalendars
            .Where(c => !c.IsDeleted)
            .Include(c => c.Template)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<UserCalendar?> GetByIdAsync(int id)
    {
        return await _context.UserCalendars
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<UserCalendar>> GetByUserIdAsync(int userId, bool includeDeleted = false)
    {
        var query = _context.UserCalendars
            .Where(c => c.UserId == userId);

        if (!includeDeleted)
        {
            query = query.Where(c => !c.IsDeleted);
        }

        return await query
            .Include(c => c.Template)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<UserCalendar?> GetByIdAndUserIdAsync(int id, int userId)
    {
        return await _context.UserCalendars
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
    }

    public async Task<bool> UserOwnsCalendarAsync(int calendarId, int userId)
    {
        return await _context.UserCalendars
            .AnyAsync(c => c.Id == calendarId && c.UserId == userId);
    }

    public async Task AddAsync(UserCalendar entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.UserCalendars.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserCalendar entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.UserCalendars.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.UserCalendars.FindAsync(id);
        if (entity is not null)
        {
            _context.UserCalendars.Remove(entity);
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
            _context.UserCalendars.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
