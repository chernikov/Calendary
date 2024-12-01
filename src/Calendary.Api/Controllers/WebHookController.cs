using Calendary.Core.Helpers;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebHookController(IWebHookService webHookService, 
        ITrainingService trainingService, 
        IFluxModelService fluxModelService,
        IDefaultJobHelper defaultJobHelper) : ControllerBase
{
    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> WebHookAsync()
    {
        var body = await SaveWebHook(webHookService);

        var webhookRequest = JsonSerializer.Deserialize<WebhookRequest>(body);

        if (webhookRequest is not null)
        {
            var replicateId = webhookRequest.Id;
            var newStatus = webhookRequest.Status;
            var version = GetVersion(webhookRequest.Output.Version);

            if (!string.IsNullOrEmpty(replicateId) && !string.IsNullOrEmpty(newStatus))
            {

                // Оновити статус у базі даних
                var training = await trainingService.GetByReplicateIdAsync(replicateId);

                if (training is not null)
                {
                    await trainingService.UpdateStatusAsync(training.Id, newStatus);
                    await trainingService.UpdateVersionAsync(training.Id, version);


                    var fluxModel = await fluxModelService.GetByIdAsync(training.FluxModelId);
                    if (fluxModel is not null)
                    {
                        fluxModel.Status = "processed";
                        fluxModel.Version = version;
                        await fluxModelService.UpdateStatusAsync(fluxModel);
                        await fluxModelService.UpdateVersionAsync(fluxModel);

                        // run default job
                        await defaultJobHelper.RunAsync(fluxModel.Id);
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


    [HttpGet("data/{id:int}")]
    public async Task<IActionResult> Data(int id)
    {
        var webHook = await webHookService.GetByIdAsync(id);

        if (webHook is null)
        {

            return NotFound(); 
        }
        return Ok(webHook.Body);
    }

    public string GetVersion(string outputVersion)
    {
        var version = outputVersion.Split(":")[1];
        return version;
    }
}
