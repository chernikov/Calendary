using System.Text.Json.Serialization;

namespace Calendary.Core.Senders.Models;

internal class SuccessInfo
{
    [JsonPropertyName("info")]
    public Dictionary<string, string>? Info { get; set; }

    [JsonPropertyName("add_info")]
    public Dictionary<string, string>? AddInfo { get; set; }
}
