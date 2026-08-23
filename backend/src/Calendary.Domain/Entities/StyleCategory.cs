namespace Calendary.Domain.Entities;

public class StyleCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int SortOrder { get; set; }
}
