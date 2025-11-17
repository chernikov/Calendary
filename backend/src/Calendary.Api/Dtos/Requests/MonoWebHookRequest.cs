using System.Text.Json.Serialization;

namespace Calendary.Api.Dtos.Requests;

public class MonoWebHookRequest
{
    [JsonPropertyName("invoiceId")]
    public string InvoiceId { get; set; } = null!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("payMethod")]
    public string PayMethod { get; set; } = null!;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("ccy")]
    public int Ccy { get; set; }

    [JsonPropertyName("finalAmount")]
    public int FinalAmount { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("modifiedDate")]
    public DateTime ModifiedDate { get; set; }

    [JsonPropertyName("paymentInfo")]
    public MonoWebHookPaymentInfo PaymentInfo { get; set; } = null!;
}


public class MonoWebHookPaymentInfo
{
    [JsonPropertyName("rrn")]
    public string Rrn { get; set; } = null!;

    [JsonPropertyName("approvalCode")]
    public string ApprovalCode { get; set; } = null!;

    [JsonPropertyName("tranId")]
    public string TranId { get; set; } = null!;

    [JsonPropertyName("terminal")]
    public string Terminal { get; set; } = null!;

    [JsonPropertyName("bank")]
    public string Bank { get; set; } = null!;

    [JsonPropertyName("paymentSystem")]
    public string PaymentSystem { get; set; } = null!;

    [JsonPropertyName("country")]
    public string Country { get; set; } = null!;

    [JsonPropertyName("fee")]
    public int Fee { get; set; }

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; } = null!;

    [JsonPropertyName("maskedPan")]
    public string MaskedPan { get; set; } = null!;
}
