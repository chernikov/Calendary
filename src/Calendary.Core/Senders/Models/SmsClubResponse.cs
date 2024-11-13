using System.Text.Json.Serialization;

namespace Calendary.Core.Senders.Models;

internal class SmsClubResponse
{
    [JsonPropertyName("success_request")]
    public SuccessInfo? SuccessRequest { get; set; }
}
