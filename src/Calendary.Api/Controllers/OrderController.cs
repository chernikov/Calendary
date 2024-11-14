using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : BaseUserController
{
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;

    public OrderController(IUserService userService, IOrderService orderService, IMapper mapper) : base(userService)
    {
        _orderService = orderService;
        _mapper = mapper;
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderDetails(int orderId)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var order = await _orderService.GetFullCreatingOrderAsync(user.Id);

        if (order == null)
        {
            return NotFound("Замовлення не знайдено.");
        }

        // Перевірка належності замовлення користувачу
        if (order.UserId != user.Id)
        {
            return Forbid("Замовлення не належить поточному користувачу.");
        }

        var result = _mapper.Map<SummaryOrderDto>(order);
        return Ok(result);
    }
}