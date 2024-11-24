using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;


public class TrainModelUrls
{
    [JsonPropertyName("cancel")]
    public string Cancel { get; set; }

    [JsonPropertyName("get")]
    public string Get { get; set; }

    [JsonPropertyName("stream")]
    public string Stream { get; set; }
}