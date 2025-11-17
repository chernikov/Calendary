namespace Calendary.Model;

public class MonoWebhookEvent
{
    public int Id { get; set; }
    public string EventType { get; set; } = null!;
    public string Data { get; set; } = null!;

    public string XSign { get; set;} = null!; 
    public DateTime ReceivedAt { get; set; }
}
