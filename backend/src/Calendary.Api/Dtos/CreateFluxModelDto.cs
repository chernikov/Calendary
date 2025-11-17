namespace Calendary.Api.Dtos;

public class CreateFluxModelDto
{
    public int? CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;
}
