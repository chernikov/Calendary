using System.Net.Http.Headers;
using System.Net.Http.Json;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Calendary.Infrastructure.Services;

/// Calls SMS.Club's API (https://smsclub.mobi/en/api/) directly over HTTP — same minimal-deps
/// approach as ResendEmailService/NovaPoshtaService/MonobankPaymentService. Unlike those, SMS.Club
/// has no documented sandbox mode: every real send costs money and reaches an actual phone. So
/// rather than "unconfigured = settle locally" (Monobank) or "unconfigured = log the body"
/// (Resend), the fallback here is a fixed, never-sent "0000" code (#304) — used whenever
/// SmsClub:ApiKey is blank (local dev always) or, on staging, whenever an admin hasn't opted into
/// AppSettings.RealIntegrationsOnStaging for one-off testing (#432 follow-up) — Production always
/// uses the real key regardless of that flag, so it can never end up accidentally disabled there.
public class SmsClubService(
    HttpClient httpClient, IOptions<SmsClubOptions> options, IAppSettingsService settings, IHostEnvironment env,
    ILogger<SmsClubService> logger) : ISmsService
{
    private const string SendUrl = "https://im.smsclub.mobi/sms/send";
    private const string FixedFallbackCode = "0000";
    private readonly SmsClubOptions _options = options.Value;

    public async Task<string> SendVerificationCodeAsync(string phone, CancellationToken ct = default)
    {
        var configured = !string.IsNullOrWhiteSpace(_options.ApiKey);
        var useReal = configured && (env.IsProduction() || await settings.GetRealIntegrationsOnStagingAsync(ct));
        if (!useReal)
        {
            logger.LogInformation(
                "SmsClub real sending is off for this environment — phone verification code is the fixed {Code} (no real SMS sent) for {Phone}",
                FixedFallbackCode, phone);
            return FixedFallbackCode;
        }

        var code = Random.Shared.Next(0, 10_000).ToString("D4");
        var payload = new
        {
            phone = new[] { phone.TrimStart('+') },
            message = $"Код підтвердження Calendary: {code}",
            src_addr = _options.SenderName,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, SendUrl) { Content = JsonContent.Create(payload) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("SmsClub send failed for {Phone}: {Status} {Body}", phone, response.StatusCode, body);
            throw new InvalidOperationException("Failed to send verification SMS.");
        }

        return code;
    }
}
