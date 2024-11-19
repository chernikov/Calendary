namespace Calendary.Model;

public class WebHook
{
    public int Id { get; set; }
    public string Method { get; set; } = string.Empty; // GET або POST
    public string Headers { get; set; } = string.Empty; // Заголовки
    public string QueryString { get; set; } = string.Empty; // QueryString (для GET)
    public string Body { get; set; } = string.Empty; // Тіло запиту (для POST)
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow; // Час отримання

    public string EntityType { get; set; } = string.Empty; // Тип об'єкта (наприклад, "FluxModel", "AnotherEntity")

    public int? EntityId { get; set; } // Ідентифікатор пов'язаної сутності
}
