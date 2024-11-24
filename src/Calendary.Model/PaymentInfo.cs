using System.ComponentModel.DataAnnotations;

namespace Calendary.Model;

public class PaymentInfo
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    [MaxLength(10)]
    public string PaymentMethod { get; set; } = null!; // "CreditCard", "PayPal"
    public bool IsPaid { get; set; }
    public DateTime PaymentDate { get; set; }
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
    public int? FluxModelId { get; set; }
    public FluxModel? FluxModel { get; set; } 
}
