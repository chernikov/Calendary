namespace Calendary.Api.Dtos.Requests;

public class MonoWebHookRequest
{
    public string InvoiceId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string PayMethod { get; set; } = null!;
    public int Amount { get; set; }
    public int Ccy { get; set; }
    public int FinalAmount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public MonoWebHookPaymentInfo PaymentInfo { get; set; } = null!;
}


public class MonoWebHookPaymentInfo
{
    public string Rrn { get; set; } = null!;
    public string ApprovalCode { get; set; } = null!;
    public string TranId { get; set; } = null!;
    public string Terminal { get; set; } = null!;
    public string Bank { get; set; } = null!;
    public string PaymentSystem { get; set; } = null!;
    public string Country { get; set; } = null!;
    public int Fee { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string MaskedPan { get; set; } = null!;
}
