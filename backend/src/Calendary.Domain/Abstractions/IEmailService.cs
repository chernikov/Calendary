namespace Calendary.Domain.Abstractions;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string html, CancellationToken ct = default);
}
