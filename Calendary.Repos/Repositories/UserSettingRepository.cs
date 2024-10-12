using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public class UserSettingRepository : IRepository<UserSetting>
{
    private readonly ICalendaryDbContext _context;

    public UserSettingRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserSetting>> GetAllAsync()
    {
        return await _context.UserSettings.ToListAsync();
    }

    public async Task<UserSetting?> GetByIdAsync(int id)
    {
        return await _context.UserSettings.FindAsync(id);
    }

    public async Task AddAsync(UserSetting entity)
    {
        await _context.UserSettings.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserSetting entity)
    {
        _context.UserSettings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.UserSettings.FindAsync(id);
        if (entity is not null)
        {
            _context.UserSettings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
