using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Api.Dtos.Admin;
using Calendary.Api.Dtos.Results;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/[controller]")]
public class OrderController(IUserService userService, IOrderService orderService, IMapper mapper) : BaseAdminController(userService)
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // Отримуємо дані замовлень зі зв'язаними сутностями
        var (orders, totalCount) = await orderService.GetAllOrdersWithPagingAsync(page, pageSize);

        // Формуємо результат для клієнта
        var result = new AdminOrderResult
        {
            Orders = mapper.Map<IReadOnlyCollection<AdminOrderDto>>(orders),
            Total = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }


    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { Message = "Замовлення не знайдено" });
        }

        order.Status = dto.Status;
        
        await orderService.UpdateOrderStatusAsync(order);

        return Ok(new { Message = "Статус замовлення оновлено" });
    }
}