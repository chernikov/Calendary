using Calendary.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

/// Receives Monobank's asynchronous invoice-status callbacks (see MonobankPaymentService).
/// Anonymous by necessity — Monobank has no bearer token for our app, only its own X-Sign
/// signature, which HandleWebhookAsync verifies against the merchant's public key.
[ApiController]
[Route("api/payments/monobank")]
[AllowAnonymous]
public class PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        var signature = Request.Headers["X-Sign"].FirstOrDefault();
        var handled = await paymentService.HandleWebhookAsync(body, signature);
        if (!handled)
        {
            logger.LogWarning("Monobank webhook: rejected (bad signature or unknown invoice)");
            return BadRequest();
        }

        return Ok();
    }
}
