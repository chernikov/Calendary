namespace Calendary.Domain.Entities;

public class PersonalDate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public int Day { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = default!;
}
