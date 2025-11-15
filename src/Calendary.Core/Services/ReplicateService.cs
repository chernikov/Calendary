using Calendary.Core.Providers;
using Calendary.Core.Services.Models;
using Calendary.Core.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Calendary.Core.Services;

public interface IReplicateService
{
    Task<CreateModelResponse> CreateModelAsync(string modelName, string description);

    Task<TrainModelResponse> TrainModelAsync(string destination, TrainModelRequestInput input);

    Task<string> StartImageGenerationAsync(string modelVersion, GenerateImageInput input);

    Task<GenerateImageResponse> GenerateImageAsync(string predictionId);

    Task<GenerateImageResponse> GenerateImageAsync(string predictionId, Func<ProgressUpdate, Task>? onProgress);

    Task CancelTrainingAsync(string replicateId);

    Task<TrainModelResponse> GetTrainingStatusAsync(string replicateId);

    Task<GenerateImageResponse> GetImageGenerationStatusAsync(string replicateId);

    Task<string> DownloadAndSaveImageAsync(string imageUrl);
}

public class ReplicateService : IReplicateService
{
    private readonly HttpClient _httpClient;
    private readonly IPathProvider _pathProvider;
    private readonly ReplicateSettings _settings;

    public ReplicateService(HttpClient httpClient, 
            IOptions<ReplicateSettings> settings,
            IPathProvider pathProvider)
    {
        _httpClient = httpClient;
        _pathProvider = pathProvider;
        _settings = settings.Value;
    }

    private void AddAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
    }

    // 1. Створення моделі
    public async Task<CreateModelResponse> CreateModelAsync(string modelName, string description)
    {
        AddAuthorizationHeader();

        var requestBody = new CreateModelRequest
        {
            Owner = _settings.Owner,
            Name = modelName,
            Description = description,
            Visibility = "private",
            Hardware = "cpu"
        };


        var json = JsonSerializer.Serialize(requestBody);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.replicate.com/v1/models", stringContent);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreateModelResponse>(content)!;
    }

    // 2. Тренування моделі
    public async Task<TrainModelResponse> TrainModelAsync(string destination, TrainModelRequestInput input)
    {
        AddAuthorizationHeader();

        var requestBody = new TrainModelRequest
        {
            Destination = destination,
            Input = input,
            Webhook = _settings.WebhookUrl
        };

        var url = $"https://api.replicate.com/v1/models/{_settings.TrainerModel}/versions/{_settings.TrainerVersion}/trainings";

        var response = await _httpClient.PostAsync(
            url,
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TrainModelResponse>(content)!;
    }

    // 3. Генерація зображень
    public async Task<string> StartImageGenerationAsync(string modelVersion, GenerateImageInput input)
    {
        AddAuthorizationHeader();

        var requestBody = new GenerateImageRequest
        {
            Version = modelVersion,
            Input = input
        };

        var json = JsonSerializer.Serialize(requestBody);
        var reqContent = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.replicate.com/v1/predictions")
        {
            Content = reqContent
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GenerateImageResponse>(content)!;
        return result.Id;
    }

    public async Task<GenerateImageResponse> GenerateImageAsync(string predictionId)
    {
        return await GenerateImageAsync(predictionId, null);
    }

    public async Task<GenerateImageResponse> GenerateImageAsync(string predictionId, Func<ProgressUpdate, Task>? onProgress)
    {
        var attempts = 0;
        var startTime = DateTime.UtcNow;

        // Відправити початковий прогрес
        if (onProgress != null)
        {
            await onProgress(new ProgressUpdate
            {
                Progress = 0,
                Status = "Starting...",
                EstimatedTime = null
            });
        }

        while (attempts < _settings.MaxRetries)
        {
            var status = await GetImageGenerationStatusAsync(predictionId);

            // Розрахунок прогресу на основі статусу
            var progress = CalculateProgress(status.Status, attempts, _settings.MaxRetries);
            var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            var estimatedTotal = attempts > 0 ? (elapsed / attempts) * _settings.MaxRetries : null;
            var estimatedRemaining = estimatedTotal.HasValue ? (int)(estimatedTotal.Value - elapsed) : null;

            // Відправити оновлення прогресу
            if (onProgress != null)
            {
                await onProgress(new ProgressUpdate
                {
                    Progress = progress,
                    Status = GetStatusMessage(status.Status),
                    EstimatedTime = estimatedRemaining
                });
            }

            if (status.Status == "succeeded")
            {
                // Відправити фінальний прогрес
                if (onProgress != null)
                {
                    await onProgress(new ProgressUpdate
                    {
                        Progress = 100,
                        Status = "Completed!",
                        EstimatedTime = 0
                    });
                }
                return status;
            }

            if (status.Status == "failed")
            {
                // Відправити повідомлення про помилку
                if (onProgress != null)
                {
                    await onProgress(new ProgressUpdate
                    {
                        Progress = 0,
                        Status = "Failed",
                        Error = status.Logs
                    });
                }
                throw new Exception($"Image generation failed: {status.Logs}");
            }

            await Task.Delay(1000);
            attempts++;
        }

        throw new TimeoutException("Image generation timed out.");
    }

    private int CalculateProgress(string status, int attempts, int maxRetries)
    {
        return status switch
        {
            "starting" => 10,
            "processing" => 10 + (int)((attempts / (double)maxRetries) * 80),
            _ => 5
        };
    }

    private string GetStatusMessage(string status)
    {
        return status switch
        {
            "starting" => "Starting generation...",
            "processing" => "Processing image...",
            "succeeded" => "Completed!",
            "failed" => "Failed",
            _ => "Waiting..."
        };
    }


    /// <summary>
    /// Скасовує тренування за replicateId.
    /// </summary>
    /// <param name="replicateId">Replicate ID тренування</param>
    public async Task CancelTrainingAsync(string replicateId)
    {
        AddAuthorizationHeader();

        var response = await _httpClient.PostAsync($"https://api.replicate.com/v1/predictions/{replicateId}/cancel", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Отримує статус тренування за replicateId.
    /// </summary>
    /// <param name="replicateId">Replicate ID тренування</param>
    /// <returns>Статус тренування</returns>
    public async Task<TrainModelResponse> GetTrainingStatusAsync(string replicateId)
    {
        AddAuthorizationHeader();

        var response = await _httpClient.GetAsync($"https://api.replicate.com/v1/predictions/{replicateId}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TrainModelResponse>();
        return result ?? throw new InvalidOperationException("Failed to deserialize training status response.");
    }

    public async Task<GenerateImageResponse> GetImageGenerationStatusAsync(string replicateId)
    {
        AddAuthorizationHeader();

        var response = await _httpClient.GetAsync($"https://api.replicate.com/v1/predictions/{replicateId}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GenerateImageResponse>();
        return result ?? throw new InvalidOperationException("Failed to deserialize generate image  status response.");
    }

    public async Task<string> DownloadAndSaveImageAsync(string imageUrl)
    {
        try
        {
            // Завантаження зображення через HttpClient
            var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();

            // Отримання байтів зображення
            var imageBytes = await response.Content.ReadAsByteArrayAsync();

            // Формуємо шлях для збереження зображення
            var fileName = $"{Guid.NewGuid()}.jpg"; // Генеруємо унікальне ім'я
            var path = Path.Combine("uploads", fileName);
            var realPath = _pathProvider.MapPath(path); // Каталог для збереження

            // Збереження файлу
            await File.WriteAllBytesAsync(realPath, imageBytes);

            // Повертаємо відносний шлях (наприклад, для доступу через веб)
            return path;
        }
        catch (Exception ex)
        {
            // Логування помилки
            Console.WriteLine($"Error downloading image: {ex.Message}");
            throw;
        }
    }

 
}
