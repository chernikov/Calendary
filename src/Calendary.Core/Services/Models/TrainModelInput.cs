using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;
public class TrainModelInput
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