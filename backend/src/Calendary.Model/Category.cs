using System.ComponentModel.DataAnnotations;

namespace Calendary.Model;


public class Category
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    public bool IsAlive { get; set; } = true;
}
