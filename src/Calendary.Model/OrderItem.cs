namespace Calendary.Model;

public class OrderItem
{
    public int Id { get; set; }
    
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public int CalendarId { get; set; }

    public Calendar Calendar { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}
