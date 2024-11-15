using Calendary.Api.Dtos.Requests;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Calendary.Api.Controllers;

[Route("api/pay")]
[Authorize]
public class PaymentController : BaseUserController
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;

    public PaymentController(IUserService userService,
        IOrderService orderService, 
        IPaymentService paymentService
        ) : base(userService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> Pay()
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

        var totalAmount = order.OrderItems.Sum(p => p.Quantity * p.Price);
        var count = order.OrderItems.Sum(p => p.Quantity);
        var pageUrl = await _paymentService.CreateInvoiceAsync(order.Id, totalAmount);

        return Ok(new { paymentPage = pageUrl });
    }


    [AllowAnonymous]
    [HttpPost("mono/callback")]
    public async Task<IActionResult> MonoCallback()
    {
        try
        {
            // Читання тіла запиту
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var xSign = Request.Headers["X-Sign"].ToString() ?? string.Empty;

            // Збереження у базу даних
            await _paymentService.SaveWebhookAsync(body, xSign);
            var hook = JsonSerializer.Deserialize<MonoWebHookRequest>(body);
            if (hook is null)
            {
                return BadRequest();
            }

            // Обробка події
            var paymentInfo = await _paymentService.GetPaymentInfoByInvoiceIdAsync(hook.InvoiceId);
            if (paymentInfo is null)
            {
                return BadRequest();
            }
            if (hook.Status == "success")
            {
                paymentInfo.IsPaid = true;
                await _paymentService.UpdatePaymentInfoStatusAsync(paymentInfo);

                var order = await _orderService.GetOrderByIdAsync(paymentInfo.OrderId);
                if (order is not null)
                {
                    order.Status = "Paid";
                    order.IsPaid = true;
                    await _orderService.UpdateOrderStatusAsync(order);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            // Логування помилки
            Console.WriteLine("Error while processing webhook: " + ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }
}
