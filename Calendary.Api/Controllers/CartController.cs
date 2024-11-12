using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/cart")]
public class CartController(IUserService userService, 
        IOrderService orderService, 
        IImageService imageService,
        ICalendarService calendarService,
        IMapper mapper) : BaseUserController(userService)
{

    [HttpGet("count")]
    public async Task<IActionResult> Count()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Ok(0);
        }
        var count = await orderService.OrderItemsCountAsync(user.Id);
        return Ok(count);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }
        var order = await orderService.GetFullCreatingOrderAsync(user.Id);
        if (order is null)
        {
            return NotFound();
        }

        order.OrderItems = order.OrderItems.Where(p => p.Calendar.FilePath != null).ToList();
        var result = mapper.Map<OrderDto>(order);
        return Ok(result);
    }


    [HttpDelete("{itemId:int}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        // Отримуємо елемент замовлення за його ID
        var orderItem = await orderService.GetOrderItemByIdAsync(itemId);
        if (orderItem is null)
        {
            return NotFound(new { message = "Order item not found." });
        }

        // Отримуємо замовлення за його ID
        var order = await orderService.GetOrderByIdAsync(orderItem.OrderId);
        if (order is null)
        {
            return NotFound(new { message = "Order not found." });
        }

        if (order.UserId != user.Id)
        {
            return Forbid("You are not authorized to delete this item.");
        }

        // Видаляємо всі зображення, пов'язані з календарем
        await imageService.DeleteImagesByCalendarIdAsync(orderItem.CalendarId);

        // Видаляємо календар
        await calendarService.DeleteAsync(orderItem.CalendarId);

        // Видаляємо елемент замовлення з корзини
        await orderService.DeleteOrderItemAsync(itemId);

        return Ok(new { message = "Item removed successfully." });
    }

    [HttpPut("item")]
    public async Task<IActionResult> UpdateItem([FromBody] OrderItemDto itemDto)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        // Отримуємо елемент замовлення за його ID
        var orderItem = await orderService.GetOrderItemByIdAsync(itemDto.Id);
        if (orderItem is null)
        {
            return NotFound(new { message = "Order item not found." });
        }

        // Отримуємо замовлення за його ID
        var order = await orderService.GetOrderByIdAsync(orderItem.OrderId);
        if (order is null)
        {
            return NotFound(new { message = "Order not found." });
        }

        if (order.UserId != user.Id)
        {
            return Forbid("You are not authorized to delete this item.");
        }

        var updatedItem = mapper.Map<OrderItem>(itemDto);
        
        await orderService.UpdateOrderItemAsync(updatedItem);

        return Ok();
    }

}
