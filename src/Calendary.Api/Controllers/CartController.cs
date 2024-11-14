using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Calendary.Api.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/cart")]
public class CartController : BaseUserController
{
    private readonly IUserSettingService _userSettingService;
    private readonly IOrderService _orderService;
    private readonly IImageService _imageService;
    private readonly ICalendarService _calendarService;
    private readonly IMapper _mapper;

    public CartController(IUserService userService,
        IUserSettingService userSettingService,
        IOrderService orderService,
        IImageService imageService,
        ICalendarService calendarService,
        IMapper mapper) : base(userService)
    {
        _userSettingService = userSettingService;
        _orderService = orderService;
        _imageService = imageService;
        _calendarService = calendarService;
        _mapper = mapper;
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Ok(0);
        }
        var count = await _orderService.OrderItemsCountAsync(user.Id);
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
        var order = await _orderService.GetFullCreatingOrderAsync(user.Id);
        if (order is null)
        {
            return NotFound();
        }

        order.OrderItems = order.OrderItems.Where(p => p.Calendar.FilePath != null).ToList();
        var result = _mapper.Map<OrderDto>(order);
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
        var orderItem = await _orderService.GetOrderItemByIdAsync(itemId);
        if (orderItem is null)
        {
            return NotFound(new { message = "Order item not found." });
        }

        // Отримуємо замовлення за його ID
        var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
        if (order is null)
        {
            return NotFound(new { message = "Order not found." });
        }

        if (order.UserId != user.Id)
        {
            return Forbid("You are not authorized to delete this item.");
        }

        // Видаляємо всі зображення, пов'язані з календарем
        await _imageService.DeleteImagesByCalendarIdAsync(orderItem.CalendarId);

        // Видаляємо календар
        await _calendarService.DeleteAsync(orderItem.CalendarId);

        // Видаляємо елемент замовлення з корзини
        await _orderService.DeleteOrderItemAsync(itemId);

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
        var orderItem = await _orderService.GetOrderItemByIdAsync(itemDto.Id);
        if (orderItem is null)
        {
            return NotFound(new { message = "Order item not found." });
        }

        // Отримуємо замовлення за його ID
        var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
        if (order is null)
        {
            return NotFound(new { message = "Order not found." });
        }

        if (order.UserId != user.Id)
        {
            return Forbid("You are not authorized to delete this item.");
        }

        var updatedItem = _mapper.Map<OrderItem>(itemDto);

        await _orderService.UpdateOrderItemAsync(updatedItem);

        return Ok();
    }


    [HttpGet("delivery")]
    public async Task<IActionResult> GetDeliveryInfo()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var order = await _orderService.GetFullCreatingOrderAsync(user.Id);
        if (order is not null)
        {
            if (order.DeliveryRaw is not null)
            {
                var result = JsonSerializer.Deserialize<DeliveryDto>(order.DeliveryRaw);
                if (result is not null)
                {
                    return Ok(result);
                }
            }
        }
        var userSetting = await _userSettingService.GetSettingsByUserIdAsync(user.Id);
        if (userSetting is not null)
        {
            if (userSetting.DeliveryRaw is not null)
            {
                var result = JsonSerializer.Deserialize<DeliveryDto>(userSetting.DeliveryRaw!);
                if (result is not null)
                {
                    return Ok(result);
                }
            }
        }
        return Ok();
    }

    [HttpPut("delivery")]
    public async Task<IActionResult> UpdateDeliveryInfo([FromBody] DeliveryDto delivery)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }
        var raw = JsonSerializer.Serialize(delivery);
        var address = MakeAddress(delivery);
        await SaveToOrder(user, raw, address);
        await SaveToSetting(user, raw, address);
        return Ok();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var order = await _orderService.GetFullCreatingOrderAsync(user.Id);
        if (order is null)
        {
            return NotFound(new { message = "Order not found." });
        }

        var summary = _mapper.Map<SummaryOrderDto>(order);
        return Ok(summary);
    }

    private async Task SaveToSetting(User user, string raw, string address)
    {
        var userSetting = await _userSettingService.GetSettingsByUserIdAsync(user.Id);
        if (userSetting is not null)
        {
            userSetting.DeliveryAddress = address;
            userSetting.DeliveryRaw = raw;
            await _userSettingService.UpdateDeliveryAsync(userSetting.Id, userSetting);
        }
    }

    private async Task SaveToOrder(User user, string raw, string address)
    {
        var order = await _orderService.GetFullCreatingOrderAsync(user.Id);
        if (order is not null)
        {
            order.DeliveryAddress = address;
            order.DeliveryRaw = raw;
            await _orderService.UpdateDeliveryOrderAsync(order.Id, order);
        }
    }

    private string MakeAddress(DeliveryDto delivery)
        => string.Format($"{delivery.City?.Description} {delivery.PostOffice?.Description}");
}
