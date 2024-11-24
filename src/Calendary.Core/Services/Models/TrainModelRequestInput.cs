using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record TrainModelRequestInput
{
    [JsonPropertyName("steps")]
    public int Steps { get; init; }

    [JsonPropertyName("lora_rank")]
    public int LoraRank { get; init; }

    [JsonPropertyName("optimizer")]
    public string Optimizer { get; init; }

    [JsonPropertyName("batch_size")]
    public int BatchSize { get; init; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; init; }

    [JsonPropertyName("autocaption")]
    public bool Autocaption { get; init; }

    [JsonPropertyName("autocaption_prefix")]
    public string AutocaptionPrefix { get; init; }

    [JsonPropertyName("input_images")]
    public string InputImages { get; init; }

    [JsonPropertyName("trigger_word")]
    public string TriggerWord { get; init; }

    [JsonPropertyName("learning_rate")]
    public double LearningRate { get; init; }

    [JsonPropertyName("wandb_project")]
    public string WandbProject { get; init; }

    [JsonPropertyName("wandb_save_interval")]
    public int WandbSaveInterval { get; init; }

    [JsonPropertyName("caption_dropout_rate")]
    public double CaptionDropoutRate { get; init; }

    [JsonPropertyName("cache_latents_to_disk")]
    public bool CacheLatentsToDisk { get; init; }

    [JsonPropertyName("wandb_sample_interval")]
    public int WandbSampleInterval { get; init; }
}
