using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IHolidayPresetService
{
    Task<IEnumerable<HolidayPreset>> GetAllPresetsAsync();
    Task<HolidayPreset?> GetPresetByCodeAsync(string code);
    Task<IEnumerable<HolidayPreset>> GetPresetsByTypeAsync(string type);
    Task<HolidayPreset?> GetPresetWithHolidaysAsync(string code, int languageId);
    Task<bool> ApplyPresetToCalendarAsync(int calendarId, string presetCode);
}

public class HolidayPresetService : IHolidayPresetService
{
    private readonly IHolidayPresetRepository _holidayPresetRepository;
    private readonly ICalendarRepository _calendarRepository;

    public HolidayPresetService(
        IHolidayPresetRepository holidayPresetRepository,
        ICalendarRepository calendarRepository)
    {
        _holidayPresetRepository = holidayPresetRepository;
        _calendarRepository = calendarRepository;
    }

    public async Task<IEnumerable<HolidayPreset>> GetAllPresetsAsync()
    {
        return await _holidayPresetRepository.GetAllAsync();
    }

    public async Task<HolidayPreset?> GetPresetByCodeAsync(string code)
    {
        return await _holidayPresetRepository.GetByCodeAsync(code);
    }

    public async Task<IEnumerable<HolidayPreset>> GetPresetsByTypeAsync(string type)
    {
        return await _holidayPresetRepository.GetByTypeAsync(type);
    }

    public async Task<HolidayPreset?> GetPresetWithHolidaysAsync(string code, int languageId)
    {
        return await _holidayPresetRepository.GetWithHolidaysAsync(code, languageId);
    }

    public async Task<bool> ApplyPresetToCalendarAsync(int calendarId, string presetCode)
    {
        var calendar = await _calendarRepository.GetByIdAsync(calendarId);
        if (calendar == null)
        {
            return false;
        }

        var preset = await _holidayPresetRepository.GetWithHolidaysAsync(presetCode, 1); // Default to Ukrainian
        if (preset == null || preset.Holidays == null || !preset.Holidays.Any())
        {
            return false;
        }

        // Apply preset holidays to calendar by creating CalendarHoliday entries
        await _calendarRepository.AssignHolidays(calendarId, preset.Holidays);

        return true;
    }
}
