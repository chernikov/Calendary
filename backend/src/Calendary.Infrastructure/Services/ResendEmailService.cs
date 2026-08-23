using System.Net.Http.Json;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Calendary.Infrastructure.Services;

/// Calls the Resend API (https://resend.com/docs/api-reference/emails/send-email) directly over
/// HTTP — no SDK, same minimal-deps approach as the rest of this codebase's integrations.
public class ResendEmailService(HttpClient httpClient, IOptions<ResendOptions> options, ILogger<ResendEmailService> logger)
    : IEmailService
{
    private readonly ResendOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string html, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogInformation("Resend:ApiKey not configured — skipping email to {To}: {Subject}", to, subject);
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = JsonContent.Create(new
            {
                from = $"{_options.FromName} <{_options.FromEmail}>",
                to = new[] { to },
                subject,
                html
            })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Resend send to {To} failed: {Status} {Body}", to, (int)response.StatusCode, body);
        }
    }
}
