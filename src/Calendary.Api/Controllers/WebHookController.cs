using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebHookController(IWebHookService webHookService, 
        ITrainingService trainingService, 
        IFluxModelService fluxModelService) : ControllerBase
{
    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> WebHookAsync()
    {
        var body = await SaveWebHook(webHookService);

        var trainModelResponse = JsonSerializer.Deserialize<TrainModelResponse>(body);

        if (trainModelResponse is not null)
        {
            var replicateId = trainModelResponse.Id;
            var newStatus = trainModelResponse.Status;

            if (!string.IsNullOrEmpty(replicateId) && !string.IsNullOrEmpty(newStatus))
            {

                // Оновити статус у базі даних
                var trainingRecord = await trainingService.GetByReplicateIdAsync(replicateId);

                if (trainingRecord is not null)
                {
                    await trainingService.UpdateStatusAsync(trainingRecord.Id, newStatus);


                    var fluxModel = await fluxModelService.GetByIdAsync(trainingRecord.FluxModelId);
                    if (fluxModel is not null)
                    {
                        fluxModel.Status = "succeeded";
                        await fluxModelService.UpdateStatusAsync(fluxModel);
                    }

                }
            }
        }
        return Ok();
    }

    private async Task<string> SaveWebHook(IWebHookService webHookService)
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
        return body;
    }
}
