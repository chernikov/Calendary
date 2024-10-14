using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface ICalendarService
{
    Task<Calendar> CreateAsync(int userId, Calendar calendar);
    Task<Calendar?> GetCurrentAsync(int id);
}

public class CalendarService(ICalendarRepository calendarRepository, IOrderRepository orderRepository) : ICalendarService
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
}
