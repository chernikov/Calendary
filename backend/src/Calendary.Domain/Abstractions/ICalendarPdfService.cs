namespace Calendary.Domain.Abstractions;

public interface ICalendarPdfService
{
    Task<byte[]> GenerateAsync(Guid orderId, CancellationToken ct = default);
}
