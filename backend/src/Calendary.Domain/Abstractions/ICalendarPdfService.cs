namespace Calendary.Domain.Abstractions;

public interface ICalendarPdfService
{
    Task<byte[]> GenerateAsync(Guid orderId, bool watermark, CancellationToken ct = default);
}
