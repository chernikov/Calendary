using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;


public class Urls
{
    [JsonPropertyName("cancel")]
    public string Cancel { get; set; }

    [JsonPropertyName("get")]
    public string Get { get; set; }

    [JsonPropertyName("stream")]
    public string Stream { get; set; }

    public string GetReplicateId()
    {
        return Get.Split("/").Last();
    }
}