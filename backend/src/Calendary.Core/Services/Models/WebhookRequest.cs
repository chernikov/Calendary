using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record WebhookRequest
{
    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("data_removed")]
    public bool DataRemoved { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("input")]
    public WebhookRequestInput Input { get; set; }

    [JsonPropertyName("logs")]
    public string Logs { get; set; }

    [JsonPropertyName("metrics")]
    public WebhookRequestMetrics Metrics { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("output")]
    public WebhookRequestOutput Output { get; set; }

    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("urls")]
    public WebhookRequestUrls Urls { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("webhook")]
    public string Webhook { get; set; }
}

public record WebhookRequestInput
{
    [JsonPropertyName("autocaption")]
    public bool Autocaption { get; set; }

    [JsonPropertyName("autocaption_prefix")]
    public string AutocaptionPrefix { get; set; }

    [JsonPropertyName("batch_size")]
    public int BatchSize { get; set; }

    [JsonPropertyName("cache_latents_to_disk")]
    public bool CacheLatentsToDisk { get; set; }

    [JsonPropertyName("caption_dropout_rate")]
    public double CaptionDropoutRate { get; set; }

    [JsonPropertyName("input_images")]
    public string InputImages { get; set; }

    [JsonPropertyName("learning_rate")]
    public double LearningRate { get; set; }

    [JsonPropertyName("lora_rank")]
    public int LoraRank { get; set; }

    [JsonPropertyName("optimizer")]
    public string Optimizer { get; set; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; }

    [JsonPropertyName("steps")]
    public int Steps { get; set; }

    [JsonPropertyName("trigger_word")]
    public string TriggerWord { get; set; }

    [JsonPropertyName("wandb_project")]
    public string WandbProject { get; set; }

    [JsonPropertyName("wandb_sample_interval")]
    public int WandbSampleInterval { get; set; }

    [JsonPropertyName("wandb_save_interval")]
    public int WandbSaveInterval { get; set; }
}

public record WebhookRequestMetrics
{
    [JsonPropertyName("predict_time")]
    public double PredictTime { get; set; }
}

public record WebhookRequestOutput
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("weights")]
    public string Weights { get; set; }
}

public record WebhookRequestUrls
{
    [JsonPropertyName("cancel")]
    public string Cancel { get; set; }

    [JsonPropertyName("get")]
    public string Get { get; set; }

    [JsonPropertyName("stream")]
    public string Stream { get; set; }
}
