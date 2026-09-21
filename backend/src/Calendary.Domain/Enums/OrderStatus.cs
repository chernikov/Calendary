namespace Calendary.Domain.Enums;

// Stored as a plain int by ordinal position (no HasConversion) — new members must always be
// appended at the end. Inserting one in the middle (e.g. between Printing and Shipped) would
// shift the underlying int of every later member and silently corrupt the status of existing
// rows in the DB (#434).
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
    GenerationFailed,
    PrintReady,
}
