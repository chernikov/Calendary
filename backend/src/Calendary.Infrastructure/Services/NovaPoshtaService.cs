using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Calendary.Infrastructure.Services;

/// Calls Nova Poshta's public Address API (https://api.novaposhta.ua/v2.0/json/) directly over
/// HTTP — same minimal-deps approach as ResendEmailService. Falls back to the small static
/// dataset MockNovaPoshtaService used to carry when no API key is configured, so local dev keeps
/// working without one.
public class NovaPoshtaService(HttpClient httpClient, IOptions<NovaPoshtaOptions> options, ILogger<NovaPoshtaService> logger)
    : INovaPoshtaService
{
    private const string ApiUrl = "https://api.novaposhta.ua/v2.0/json/";
    private readonly NovaPoshtaOptions _options = options.Value;

    private static readonly string[] FallbackCities =
    [
        "Львів", "Київ", "Харків", "Одеса", "Дніпро", "Вінниця", "Івано-Франківськ", "Тернопіль"
    ];

    private static readonly Dictionary<string, NovaPoshtaWarehouse[]> FallbackWarehouses = new()
    {
        ["Львів"] =
        [
            new("№12", "вул. Городоцька, 359", "до 20:00"),
            new("№34", "вул. Липинського, 54", "до 21:00"),
            new("№81", "пр. Червоної Калини, 62", "до 20:00")
        ],
        ["Київ"] =
        [
            new("№1", "вул. Хрещатик, 22", "до 22:00"),
            new("№47", "просп. Перемоги, 100", "до 21:00"),
            new("№103", "вул. Драгоманова, 14", "до 20:00")
        ]
    };

    private static readonly NovaPoshtaWarehouse[] DefaultFallbackWarehouses =
    [
        new("№1", "центральне відділення", "до 20:00"),
        new("№5", "вул. Соборна, 10", "до 20:00"),
        new("№18", "вул. Незалежності, 3", "до 19:00")
    ];

    public async Task<IReadOnlyList<string>> SearchCitiesAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return FallbackCities
                .Where(c => string.IsNullOrWhiteSpace(query) || c.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var data = await CallAsync("getCities", new { FindByString = query, Limit = "20" }, ct);
        if (data is null) return [];

        return data
            .Select(item => item.TryGetProperty("Description", out var d) ? d.GetString() : null)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();
    }

    public async Task<IReadOnlyList<NovaPoshtaWarehouse>> GetWarehousesAsync(string city, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return FallbackWarehouses.TryGetValue(city, out var w) ? w : DefaultFallbackWarehouses;
        }

        var data = await CallAsync("getWarehouses", new { CityName = city, Limit = "50" }, ct);
        if (data is null) return [];

        return data
            .Select(item =>
            {
                var number = item.TryGetProperty("Number", out var n) ? n.GetString() : null;
                var address = item.TryGetProperty("ShortAddress", out var a) && !string.IsNullOrWhiteSpace(a.GetString())
                    ? a.GetString()
                    : item.TryGetProperty("Description", out var d) ? d.GetString() : null;
                return new NovaPoshtaWarehouse(number ?? "?", address ?? "", ParseClosesAt(item));
            })
            .ToList();
    }

    private async Task<List<JsonElement>?> CallAsync(string method, object methodProperties, CancellationToken ct)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                ApiUrl,
                new { apiKey = _options.ApiKey, modelName = "Address", calledMethod = method, methodProperties },
                ct);

            var envelope = await response.Content.ReadFromJsonAsync<NovaPoshtaResponse>(cancellationToken: ct);
            if (envelope is null || !envelope.Success)
            {
                logger.LogWarning(
                    "Nova Poshta {Method} failed: {Errors}",
                    method,
                    envelope?.Errors is { Count: > 0 } errors ? string.Join("; ", errors) : "unknown response shape");
                return null;
            }
            return envelope.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Nova Poshta {Method} threw", method);
            return null;
        }
    }

    // Nova Poshta's exact `Schedule` shape wasn't confidently verifiable against a live response
    // while this was written (no API key available yet — see #290 PR description). This tries
    // the commonly-documented "weekday number -> hours string" shape and otherwise falls back to
    // a generic pointer rather than guess at a field mapping that might be wrong.
    private static string ParseClosesAt(JsonElement item)
    {
        if (item.TryGetProperty("Schedule", out var schedule) && schedule.ValueKind == JsonValueKind.Object)
        {
            var isoWeekday = (int)DateTime.Now.DayOfWeek;
            var todayKey = (isoWeekday == 0 ? 7 : isoWeekday).ToString();
            if (schedule.TryGetProperty(todayKey, out var hours) && hours.ValueKind == JsonValueKind.String)
            {
                var text = hours.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var closing = text.Split('-').LastOrDefault()?.Trim();
                    return string.IsNullOrWhiteSpace(closing) ? text : $"до {closing}";
                }
            }
        }
        return "Див. розклад на сайті Нової Пошти";
    }

    private record NovaPoshtaResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("data")] List<JsonElement>? Data,
        [property: JsonPropertyName("errors")] List<string>? Errors);
}
