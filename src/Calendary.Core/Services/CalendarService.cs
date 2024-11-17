using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface ICalendarService
{
    Task<Calendar> CreateAsync(int userId, Calendar calendar);

    Task<Calendar?> GetByIdAsync(int calendarId);

    Task<Calendar?> GetCurrentAsync(int id);

    Task<bool> UpdateCalendarAsync(int userId, Calendar entity);

    Task GeneratePdfAsync(int userId, int calendarId);

    Task MakeNotCurrentAsync(int userId, int calendarId);

    Task DeleteAsync(int calendarId);

    Task UpdatePreviewPathAsync(int calendarId, string thumbnailPath);
}

public class CalendarService(
    ICalendarRepository calendarRepository,
    IUserSettingRepository userSettingRepository,
    IOrderRepository orderRepository,
    IEventDateRepository eventDateRepository,
    IOrderItemRepository orderItemRepository,
    IHolidayRepository holidayRepository,
    IPriceService priceService,
    IPdfGeneratorService pdfGeneratorService) 
    : ICalendarService
{

    public async Task<Calendar> CreateAsync(int userId, Calendar calendar)
    {
        // Перевіряємо, чи є існуюче замовлення зі статусом "Creating"
        var currentOrder = await orderRepository.GetOrderByStatusAsync(userId, "Creating");

        // Якщо замовлення немає, створюємо нове
        if (currentOrder is null)
        {
            currentOrder = new Order
            {
                UserId = userId,
                Status = "Creating",
                OrderDate = DateTime.UtcNow,
            };

            var userSetting = await userSettingRepository.GetByUserIdAsync(userId);

            if (userSetting is not null)
            {
                currentOrder.DeliveryAddress = userSetting.DeliveryAddress;
                currentOrder.DeliveryRaw = userSetting.DeliveryRaw;
            }
            await orderRepository.AddAsync(currentOrder);
        }

        calendar.UserId = userId;

        await calendarRepository.AddAsync(calendar);

        // Прив'язуємо календар до замовлення
        var orderItem = new OrderItem
        {
            OrderId = currentOrder.Id,
            CalendarId = calendar.Id,
            Quantity = 1,
            Price = priceService.GetPrice()
        };

        await orderItemRepository.AddAsync(orderItem);
        await MakeCurrentAsync(userId, calendar.Id);

        return calendar;
    }

    public Task<Calendar?> GetByIdAsync(int calendarId)
    {
        return calendarRepository.GetFullCalendarAsync(calendarId);
    }

    public async Task<Calendar?> GetCurrentAsync(int userId)
    {
        var userCalendars = await calendarRepository.GetCalendarsByUserAsync(userId);
        return userCalendars.ToList().FirstOrDefault(p => p.IsCurrent);
    }

    public async Task MakeCurrentAsync(int userId, int calendarId)
    {
        // Отримуємо календар
        var calendar = await calendarRepository.GetByIdAsync(calendarId);
        if (calendar is not null && calendar.UserId == userId)
        {
            calendar.IsCurrent = (calendar.Id == calendarId);
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

        if (existingCalendar.UserId != userId)
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

        if (calendar is null || calendar.UserId != userId)
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

    public async Task MakeNotCurrentAsync(int userId, int calendarId)
    {
        var calendar = await calendarRepository.GetFullCalendarAsync(calendarId);

        if (calendar is not null && calendar.UserId == userId)
        {
            calendar.IsCurrent = false;
            await calendarRepository.UpdateAsync(calendar);
        }
    }

    public Task DeleteAsync(int calendarId)
        => calendarRepository.DeleteAsync(calendarId);

    public Task UpdatePreviewPathAsync(int calendarId, string thumbnailPath)
        => calendarRepository.UpdatePreviewPathAsync(calendarId, thumbnailPath);
}
