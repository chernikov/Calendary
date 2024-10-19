using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IHolidayService
{
    Task<IEnumerable<Holiday>> GetAllHolidaysAsync();
    Task<Holiday> AddHolidayAsync(Holiday holiday);
    Task<Holiday> UpdateHolidayAsync(Holiday holiday);
    Task DeleteHolidayAsync(int id);
}    

public class HolidayService(IHolidayRepository holidayRepository) : IHolidayService
{
    public Task<IEnumerable<Holiday>> GetAllHolidaysAsync()
    {
        return holidayRepository.GetAllAsync();
    }

    public async Task<Holiday> AddHolidayAsync(Holiday holiday)
    {
        await holidayRepository.AddAsync(holiday);
        return holiday;
    }

    public async Task<Holiday> UpdateHolidayAsync(Holiday holiday)
    {

        await holidayRepository.UpdateAsync(holiday);
        return holiday;
    }

    public Task DeleteHolidayAsync(int id)
    {
        return holidayRepository.DeleteAsync(id);
    }

   
}

