using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;


public interface ICalendarRepository : IRepository<Calendar>
{
    Task AssignEventDatesAsync(int calendarId, IEnumerable<EventDate> eventDates);
    Task AssignHolidays(int calendarId, IEnumerable<Holiday> holidays);
    Task<IList<Calendar>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Calendar>> GetCalendarsByUserAsync(int userId);

    Task<Calendar?> GetFullCalendarAsync(int id);
    Task SaveFileAsync(int calendarId, string pdfFile);
    Task UpdatePreviewPathAsync(int calendarId, string thumbnailPath);
}

public class CalendarRepository : ICalendarRepository
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

    public async Task<IList<Calendar>> GetByUserIdAsync(int userId)
    {
        return await _context.Calendars.Where(c => c.UserId == userId).ToListAsync();
    }

    public async Task AddAsync(Calendar entity)
    {
        entity.CountryId = Country.Ukraine.Id;
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

    public async Task<IEnumerable<Calendar>> GetCalendarsByUserAsync(int userId)
    {
        return await _context.Calendars.Where(p => p.UserId == userId).ToListAsync();
    }

    public async Task<Calendar?> GetFullCalendarAsync(int id)
    {
        return await _context.Calendars
            .Include(p => p.Language)
            .Include(p => p.CalendarHolidays)
            .ThenInclude(p => p.Holiday)
            .Include(p => p.EventDates)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AssignEventDatesAsync(int calendarId, IEnumerable<EventDate> eventDates)
    {
        foreach(var entity in eventDates)
        {
            var newEntity = new EventDate()
            {
                Date = entity.Date,
                Description = entity.Description,
                CalendarId = calendarId
            };
            _context.EventDates.Add(newEntity);
        }

        await _context.SaveChangesAsync();
    }

    public async Task AssignHolidays(int calendarId, IEnumerable<Holiday> holidays)
    {
        foreach (var entity in holidays)
        {
            var reference = new CalendarHoliday()
            {
                CalendarId = calendarId,
                HolidayId = entity.Id                
            };
            _context.CalendarHolidays.Add(reference);
        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveFileAsync(int calendarId, string pdfFile)
    {
        var calendar = await _context.Calendars.FindAsync(calendarId);
        if (calendar != null)
        {
            calendar.FilePath = pdfFile;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdatePreviewPathAsync(int calendarId, string thumbnailPath)
    {
        var calendar = await _context.Calendars.FindAsync(calendarId);
        if (calendar != null)
        {
            calendar.PreviewPath = thumbnailPath;
            await _context.SaveChangesAsync();
        }
    }

   
}