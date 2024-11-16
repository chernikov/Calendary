using Calendary.Model;
using Calendary.Repos.Repositories;
using iText.Layout.Borders;

namespace Calendary.Core.Services;

public interface IOrderService
{
    Task<Order?> GetFullCreatingOrderAsync(int userId);

    Task<Order?> GetFullOrderAsync(int orderId);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<OrderItem?> GetOrderItemByIdAsync(int itemId);
    Task DeleteOrderItemAsync(int itemId);
    Task UpdateOrderItemAsync(OrderItem orderItem);
    Task<int> OrderItemsCountAsync(int userId);
    Task UpdateOrderDeliveryAsync(Order updatedOrder);
    Task UpdateOrderStatusAsync(Order updatedOrder);

    Task<(List<Order>, int)> GetOrdersWithPagingAsync(int userId, int page, int pageSize);

    Task<(List<Order>, int)> GetAllOrdersWithPagingAsync(int page, int pageSize);
}

internal class OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository) : IOrderService
{
    public Task<Order?> GetFullCreatingOrderAsync(int userId)
        => orderRepository.GetFullOrderByStatusAsync(userId, "Creating");

    public Task<Order?> GetFullOrderAsync(int orderId)
      => orderRepository.GetFullOrderAsync(orderId);

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

    public async Task UpdateOrderDeliveryAsync(Order updatedOrder)
    {
        var existingOrder = await orderRepository.GetByIdAsync(updatedOrder.Id);
        if (existingOrder is null)
        {
            return;
        }

        existingOrder.DeliveryAddress = updatedOrder.DeliveryAddress;
        existingOrder.DeliveryRaw = updatedOrder.DeliveryRaw;

        await orderRepository.UpdateAsync(existingOrder);
    }

    public async Task UpdateOrderStatusAsync(Order updatedOrder)
    {
        var existingOrder = await orderRepository.GetByIdAsync(updatedOrder.Id);
        if (existingOrder is null)
        {
            return;
        }

        existingOrder.IsPaid = updatedOrder.IsPaid;
        existingOrder.Status = updatedOrder.Status;
        existingOrder.UpdatedDate = DateTime.UtcNow;

        await orderRepository.UpdateAsync(existingOrder);
    }

    public Task<(List<Order>, int)> GetOrdersWithPagingAsync(int userId, int page, int pageSize)
        => orderRepository.GetOrdersWithPagingAsync(userId, page, pageSize);

    public Task<(List<Order>, int)> GetAllOrdersWithPagingAsync(int page, int pageSize)
        => orderRepository.GetAllOrdersWithPagingAsync(page, pageSize);

}


