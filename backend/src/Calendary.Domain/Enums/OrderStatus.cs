namespace Calendary.Domain.Enums;

public enum OrderStatus
{
    Created,
    PhotoUploaded,
    DetailsSubmitted,
    Generating,
    CoverReady,
    CoverConfirmed,
    ReviewReady,
    AwaitingPayment,
    Paid,
    Printing,
    Shipped,
    Delivered,
    Cancelled,
    GenerationFailed
}
