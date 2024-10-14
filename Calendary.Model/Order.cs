namespace Calendary.Model;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public bool IsPaid { get; set; }
    public string Status { get; set; } = "";
    public string DeliveryAddress { get; set; } = "";

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Calendar> Calendars { get; set; }
    public PaymentInfo? PaymentInfo { get; set; }
}