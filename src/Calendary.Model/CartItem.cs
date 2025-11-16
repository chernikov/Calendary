using System;

namespace Calendary.Model;

public class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CalendarId { get; set; }

    public int Quantity { get; set; } = 1;

    public PaperFormat Format { get; set; } = PaperFormat.A4;

    public PaperType PaperType { get; set; } = PaperType.Glossy;

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public UserCalendar Calendar { get; set; } = null!;
}
