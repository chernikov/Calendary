using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Api.Dtos.Results;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : BaseUserController
{
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;

    private const int PageSize = 10;

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

        var order = await _orderService.GetFullOrderAsync(orderId);

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


    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }
        var (orders, total) = await _orderService.GetOrdersWithPagingAsync(user.Id, page, PageSize);

        var result = new OrderResult()
        {
            Total = total,
            Page = page,
            PageSize = PageSize,
            Orders = _mapper.Map<IReadOnlyCollection<OrderDto>>(orders)
        };

        return Ok(result);
    }
}