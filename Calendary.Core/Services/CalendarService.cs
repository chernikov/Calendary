using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Core.Services;

public interface ICalendarService
{
    Task<Calendar> CreateAsync(int userId, Calendar calendar);

    Task<Calendar?> GetByIdAsync(int calendarId);

    Task<Calendar?> GetCurrentAsync(int id);

    Task<bool> UpdateCalendarAsync(int userId, Calendar entity);

    Task GeneratePdfAsync(int id, int calendarId);


}

public class CalendarService(
    ICalendarRepository calendarRepository,
    IOrderRepository orderRepository,
    IEventDateRepository eventDateRepository,
    IHolidayRepository holidayRepository,
    IPdfGeneratorService pdfGeneratorService
    ) : ICalendarService
{

    public async Task<Calendar> CreateAsync(int userId, Calendar calendar)
    {
        // Перевіряємо, чи є існуюче замовлення зі статусом "Creating"
        var existingOrder = await orderRepository.GetOrderByStatusAsync(userId, "Creating");

        // Якщо замовлення немає, створюємо нове
        if (existingOrder == null)
        {
            existingOrder = new Order
            {
                UserId = userId,
                Status = "Creating",
                OrderDate = DateTime.UtcNow
            };

            await orderRepository.AddAsync(existingOrder);
        }

        calendar.OrderId = existingOrder.Id;

        await calendarRepository.AddAsync(calendar);

        await MakeCurrentAsync(userId, calendar.Id);

        return calendar;
    }

    public Task<Calendar?> GetByIdAsync(int calendarId)
    {
        return calendarRepository.GetFullCalendarAsync(calendarId);
    }

    public async Task<Calendar?> GetCurrentAsync(int userId)
    {
        var order = await orderRepository.GetOrderByStatusAsync(userId, "Creating");
        // Якщо замовлення немає, створюємо нове
        if (order is not null)
        {
            var calendarsInOrder = await calendarRepository.GetCalendarsByOrderAsync(order.Id);

            return calendarsInOrder.ToList().FirstOrDefault(p => p.IsCurrent);
        }
        return null;
    }

    public async Task MakeCurrentAsync(int userId, int calendarId)
    {
        // Отримуємо календар
        var calendar = await calendarRepository.GetByIdAsync(calendarId);

        if (calendar == null || calendar.Order == null)
        {
            throw new Exception("Calendar or Order not found");
        }

        // Отримуємо всі календарі, прив'язані до поточного замовлення користувача
        var calendarsInOrder = await calendarRepository.GetCalendarsByOrderAsync(calendar.OrderId);

        // Оновлюємо статус всіх календарів
        foreach (var calendarItem in calendarsInOrder)
        {
            calendarItem.IsCurrent = (calendar.Id == calendarId);
            await calendarRepository.UpdateAsync(calendar);
        }
    }

    public async Task<bool> UpdateCalendarAsync(int userId, Calendar entity)
    {

        var existingCalendar = await calendarRepository.GetFullCalendarAsync(entity.Id);

        if (existingCalendar is null)
        {
            return false;
        }

        if (existingCalendar.Order.UserId != userId)
        {
            return false;
        }
        existingCalendar.LanguageId = entity.LanguageId;
        existingCalendar.FirstDayOfWeek = entity.FirstDayOfWeek;

        await calendarRepository.UpdateAsync(existingCalendar);
        return true;
    }

    public async Task GeneratePdfAsync(int userId, int calendarId)
    {
        var calendar = await calendarRepository.GetFullCalendarAsync(calendarId);

        if (calendar is null)
        {
            return;
        }

        var order = await orderRepository.GetByIdAsync(calendar.OrderId);

        if (order is null || order.UserId != userId)
        {
            return;
        }

        //do once
        if (!calendar.CalendarHolidays.Any())
        {
            // copy holidays and events to the calendar
            var eventDates = await eventDateRepository.GetAllByUserIdAsync(userId);
            var holidays = await holidayRepository.GetAllByCoutryIdAsync(calendar.CountryId);

            await calendarRepository.AssignEventDatesAsync(calendarId, eventDates);
            await calendarRepository.AssignHolidays(calendarId, holidays);
        }

        // Генеруємо PDF
        var pdfFile = await pdfGeneratorService.GeneratePdfAsync(calendarId);

        await calendarRepository.SaveFileAsync(calendarId, pdfFile);

    }
}
