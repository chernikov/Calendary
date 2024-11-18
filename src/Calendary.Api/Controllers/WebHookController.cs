using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebHookController(IWebHookService webHookService) : ControllerBase
{
    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> WebHookAsync()
    {
        // Збираємо дані із запиту
        var method = Request.Method;
        var headers = string.Join("\n", Request.Headers.Select(h => $"{h.Key}: {h.Value}"));
        var queryString = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
        string body = string.Empty;

        if (Request.ContentLength > 0 && (method == "POST" || method == "PUT"))
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            body = await reader.ReadToEndAsync();
        }

        // Створюємо об'єкт WebHook
        var webHook = new WebHook
        {
            Method = method,
            Headers = headers,
            QueryString = queryString,
            Body = body,
            ReceivedAt = DateTime.UtcNow
        };

        // Зберігаємо в базу даних
        await webHookService.AddAsync(webHook);
        return Ok();
    }
}
