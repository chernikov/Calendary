using Calendary.Core.Services.Models;
using Calendary.Core.Settings;
using Calendary.Model;
using Microsoft.Extensions.Options;
using System.Runtime;
using System.Text;
using System.Text.Json;

namespace Calendary.Core.Services;

public interface IReplicateService
{

}

public class ReplicateService : IReplicateService
{
    private readonly HttpClient _httpClient;
    private readonly ReplicateSettings _settings;

    public ReplicateService(HttpClient httpClient, IOptions<ReplicateSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    private void AddAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
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

        var response = await _httpClient.PostAsync(
            "https://api.replicate.com/v1/models",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreateModelResponse>(content)!;
    }

    // 2. Тренування моделі
    public async Task<TrainModelResponse> TrainModelAsync(string destination, TrainModelRequestInput input, string webhook)
    {
        AddAuthorizationHeader();

        var requestBody = new TrainModelRequest
        {
            Destination = destination,
            Input = input,
            Webhook = webhook
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
    public async Task<GenerateImageResponse> GenerateImageAsync(string modelVersion, GenerateImageRequestInput input)
    {
        AddAuthorizationHeader();

        var requestBody = new GenerateImageRequest
        {
            Version = modelVersion,
            Input = input
        };

        var response = await _httpClient.PostAsync(
            "https://api.replicate.com/v1/predictions",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GenerateImageResponse>(content)!;
    }
}
