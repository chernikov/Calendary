using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public bool IsPaid { get; set; }
    public string Status { get; set; } // e.g., "Pending", "Completed", "Shipped"
    public string DeliveryAddress { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public PaymentInfo PaymentInfo { get; set; } = null!;
}