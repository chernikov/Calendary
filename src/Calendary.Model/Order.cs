using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Order
{
    public int Id { get; set; }

    public DateTime UpdatedDate { get; set; } 

    public DateTime OrderDate { get; set; }

    public bool IsPaid { get; set; }

    [MaxLength(10)]
    public string Status { get; set; } = "creating";

    [MaxLength(1000)]
    public string? DeliveryAddress { get; set; } = null!;   // saved in text city, postOffice

    [MaxLength(1000)]
    public string? DeliveryRaw { get; set; } // saved in json city and postOffice

    [MaxLength(100)]
    public string? TrackingNumber { get; set; } // Nova Poshta TTN number

    public decimal? DeliveryCost { get; set; } // Delivery cost in UAH

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = [];
    
    public PaymentInfo PaymentInfo { get; set; } = null!;

    [MaxLength(1000)]
    public string? Comment { get; set; } 
}