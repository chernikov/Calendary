using System.ComponentModel.DataAnnotations;

namespace Calendary.Api.Dtos;

public class CategoryDto
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsAlive { get; set; } = true;
}
