using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class PaymentInfo
{
    public int Id { get; set; }
    public string PaymentMethod { get; set; } // "CreditCard", "PayPal"
    public bool IsPaid { get; set; }
    public DateTime PaymentDate { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }
}
