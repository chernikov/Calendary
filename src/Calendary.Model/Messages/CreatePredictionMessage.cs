namespace Calendary.Model.Messages;

public record CreatePredictionMessage
{
    public int JobId { get; set; }
    public int UserId { get; set; }
    public List<int> TaskIds { get; set; } = new List<int>();
}
