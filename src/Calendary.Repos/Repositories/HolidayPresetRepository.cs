using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IHolidayPresetRepository : IRepository<HolidayPreset>
{
    Task<HolidayPreset?> GetByCodeAsync(string code);
    Task<IEnumerable<HolidayPreset>> GetByTypeAsync(string type);
    Task<HolidayPreset?> GetWithTranslationsAsync(int id);
    Task<HolidayPreset?> GetWithHolidaysAsync(string code, int languageId);
}

public class HolidayPresetRepository : IHolidayPresetRepository
{
    private readonly ICalendaryDbContext _context;

    public HolidayPresetRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HolidayPreset>> GetAllAsync()
    {
        return await _context.HolidayPresets
            .Include(hp => hp.Translations)
                .ThenInclude(t => t.Language)
            .ToListAsync();
    }

    public async Task<HolidayPreset?> GetByIdAsync(int id)
    {
        return await _context.HolidayPresets
            .Include(hp => hp.Translations)
            .FirstOrDefaultAsync(hp => hp.Id == id);
    }

    public async Task<HolidayPreset?> GetByCodeAsync(string code)
    {
        return await _context.HolidayPresets
            .Include(hp => hp.Translations)
                .ThenInclude(t => t.Language)
            .FirstOrDefaultAsync(hp => hp.Code == code);
    }

    public async Task<IEnumerable<HolidayPreset>> GetByTypeAsync(string type)
    {
        return await _context.HolidayPresets
            .Include(hp => hp.Translations)
                .ThenInclude(t => t.Language)
            .Where(hp => hp.Type == type)
            .ToListAsync();
    }

    public async Task<HolidayPreset?> GetWithTranslationsAsync(int id)
    {
        return await _context.HolidayPresets
            .Include(hp => hp.Translations)
                .ThenInclude(t => t.Language)
            .FirstOrDefaultAsync(hp => hp.Id == id);
    }

    public async Task<HolidayPreset?> GetWithHolidaysAsync(string code, int languageId)
    {
        return await _context.HolidayPresets
            .Include(hp => hp.Translations.Where(t => t.LanguageId == languageId))
                .ThenInclude(t => t.Language)
            .Include(hp => hp.Holidays)
                .ThenInclude(h => h.Translations.Where(t => t.LanguageId == languageId))
            .FirstOrDefaultAsync(hp => hp.Code == code);
    }

    public async Task AddAsync(HolidayPreset entity)
    {
        await _context.HolidayPresets.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(HolidayPreset entity)
    {
        _context.HolidayPresets.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.HolidayPresets.FindAsync(id);
        if (entity is not null)
        {
            _context.HolidayPresets.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
