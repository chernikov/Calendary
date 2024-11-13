using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IOrderService
{
    Task<Order?> GetFullCreatingOrderAsync(int userId);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<OrderItem?> GetOrderItemByIdAsync(int itemId);
    Task DeleteOrderItemAsync(int itemId);
    Task UpdateOrderItemAsync(OrderItem orderItem);
    Task<int> OrderItemsCountAsync(int userId);
}

internal class OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository) : IOrderService
{
    public Task<Order?> GetFullCreatingOrderAsync(int userId)
        => orderRepository.GetFullOrderByStatusAsync(userId, "Creating");

    public Task<Order?> GetOrderByIdAsync(int orderId)
        => orderRepository.GetByIdAsync(orderId);
    
    public Task<OrderItem?> GetOrderItemByIdAsync(int itemId)
        => orderItemRepository.GetByIdAsync(itemId);

    public Task DeleteOrderItemAsync(int itemId)
        => orderItemRepository.DeleteAsync(itemId);

    public async Task UpdateOrderItemAsync(OrderItem orderItem)
    {
        var entity = await orderItemRepository.GetByIdAsync(orderItem.Id);
        if (entity is null)
        {
            return;
        }
        entity.Quantity = orderItem.Quantity;
        await orderItemRepository.UpdateAsync(entity);
    }

    public async Task<int> OrderItemsCountAsync(int userId)
    {
        var order = await orderRepository.GetOrderWithItemsAsync(userId, "Creating");
        if (order is null)
        {
            return 0;
        }
        var preorderedCalendar = order.OrderItems.Where(p => p.Calendar.FilePath != null).Sum(p => p.Quantity);
        return preorderedCalendar;
    }
}
