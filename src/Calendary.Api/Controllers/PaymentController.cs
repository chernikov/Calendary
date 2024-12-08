using Calendary.Api.Dtos.Requests;
using Calendary.Api.Tools;
using Calendary.Core.Services;
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
    private readonly IFluxModelService _fluxModelService;

    public PaymentController(IUserService userService,
        IOrderService orderService, 
        IPaymentService paymentService,
        IFluxModelService fluxModelService
        ) : base(userService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _fluxModelService = fluxModelService;
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

        var pageUrl = await _paymentService.CreateOrderInvoiceAsync(order.Id);

        return Ok(new { paymentPage = pageUrl });
    }

    [HttpGet("flux-model/{id:int}")]
    public async Task<IActionResult> PayFluxModel(int id)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var fluxModel = await _fluxModelService.GetUserFluxModelAsync(user.Id, id);

        if (fluxModel is null)
        {
            return NotFound();
        }

        if (fluxModel.IsPaid)
        {
            fluxModel.Status = "prepare";
            await _fluxModelService.UpdateStatusAsync(fluxModel);
        }

        var pageUrl = await _paymentService.CreateFluxInvoiceAsync(fluxModel.Id);

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

            //await ValidateSign(body, xSign);

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

                if (paymentInfo.OrderId is not null)
                {
                    var order = await _orderService.GetOrderByIdAsync(paymentInfo.OrderId!.Value);
                    if (order is not null)
                    {
                        order.Status = "Paid";
                        order.IsPaid = true;
                        await _orderService.UpdateOrderStatusAsync(order);
                    }
                }

                if (paymentInfo.FluxModelId is not null)
                {
                    var fluxModel = await _fluxModelService.GetByIdAsync(paymentInfo.FluxModelId.Value);
                    if (fluxModel is not null)
                    {
                        fluxModel.Status = "prepare";
                        fluxModel.IsPaid = true;
                        await _fluxModelService.UpdateStatusAsync(fluxModel);
                    }
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

    private async Task ValidateSign(string body, string xSign)
    {
        var publicKey = await _paymentService.GetPublicKeyAsync(false);
        if (publicKey is null)
        {
            throw new Exception("Can't get public key for validation sign");
        }
        var validate = MonoWebhookVerifier.VerifyWebhook(xSign, publicKey, body);

        if (!validate)
        {
            publicKey = await _paymentService.GetPublicKeyAsync(true);
            if (publicKey is null)
            {
                throw new Exception("Can't get public key for validation sign");
            }
            validate = MonoWebhookVerifier.VerifyWebhook(xSign, publicKey, body);
        }
        if (!validate)
        {
            throw new Exception("Sign is invalid");
        }
    }
}
