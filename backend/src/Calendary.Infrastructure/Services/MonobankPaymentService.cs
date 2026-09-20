using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Calendary.Infrastructure.Services;

/// Calls Monobank's Acquiring API (https://api.monobank.ua/api/merchant/...) directly over HTTP —
/// same minimal-deps approach as NovaPoshtaService/ResendEmailService. Falls back to settling the
/// order immediately when no merchant token is configured, so local dev needs no real account
/// (same shape as INovaPoshtaService's fallback). Unlike a synchronous charge, Monobank's flow is
/// create-invoice-then-redirect — the customer pays on Monobank's hosted page, and the outcome
/// only arrives later via HandleWebhookAsync.
public class MonobankPaymentService(HttpClient httpClient, AppDbContext db, IOptions<MonobankOptions> options, ILogger<MonobankPaymentService> logger)
    : IPaymentService
{
    private const string ApiBase = "https://api.monobank.ua";
    private readonly MonobankOptions _options = options.Value;

    // Same Nova-Poshta-discovered gotcha: System.Text.Json's default encoder escapes non-ASCII as
    // \uXXXX, which breaks some providers' JSON parsers. Not confirmed broken for Monobank, but
    // cheap to avoid pre-emptively since merchantPaymInfo carries Cyrillic text.
    private static readonly JsonSerializerOptions RequestJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    // Fetched lazily once per process and cached — Monobank's merchant public key doesn't rotate
    // in practice, and re-fetching it on every webhook would be wasteful.
    private static ECDsa? _cachedPublicKey;
    private static readonly SemaphoreSlim PublicKeyLock = new(1, 1);

    public async Task<PaymentInvoice> CreateBatchInvoiceAsync(
        IReadOnlyList<Guid> orderIds, string redirectUrl, string webHookUrl, CancellationToken ct = default)
    {
        var orders = await db.Orders.Include(o => o.Payment).Where(o => orderIds.Contains(o.Id)).ToListAsync(ct);
        if (orders.Count != orderIds.Count)
        {
            throw new InvalidOperationException("One or more orders were not found.");
        }

        foreach (var order in orders)
        {
            if (order.Payment is null)
            {
                order.Payment = new Payment { OrderId = order.Id };
                db.Payments.Add(order.Payment);
            }
            order.Payment.Method = PaymentMethod.Monobank;
            order.Payment.Amount = order.Price * order.PrintQuantity;
        }

        var totalPrice = orders.Sum(o => o.Price * o.PrintQuantity);

        if (string.IsNullOrWhiteSpace(_options.MerchantToken))
        {
            // Local-dev fallback: no real provider to redirect to, so settle immediately — same
            // "always succeeds" behavior the old MockPaymentService had.
            foreach (var order in orders)
            {
                order.Payment!.Status = PaymentStatus.Succeeded;
                order.Payment.PaidAtUtc = DateTime.UtcNow;
                order.Payment.ProviderInvoiceId = null;
                order.SetStatus(OrderStatus.Paid);
            }
            await db.SaveChangesAsync(ct);
            logger.LogInformation(
                "Monobank: no merchant token configured, settling orders [{OrderIds}] immediately (local-dev fallback)",
                string.Join(", ", orderIds));
            return new PaymentInvoice(redirectUrl);
        }

        var amountKopecks = (long)Math.Round(totalPrice * 100m, MidpointRounding.AwayFromZero);
        var reference = orders.Count == 1 ? orders[0].Id.ToString() : string.Join(",", orders.Select(o => o.Id));
        var destination = orders.Count == 1
            ? "Фотокалендар Calendary"
            : $"Фотокалендар Calendary ({orders.Count} шт.)";
        var payload = new
        {
            amount = amountKopecks,
            ccy = 980,
            merchantPaymInfo = new { reference, destination },
            redirectUrl,
            webHookUrl,
        };

        using var content = new StringContent(JsonSerializer.Serialize(payload, RequestJsonOptions), Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase}/api/merchant/invoice/create") { Content = content };
        request.Headers.Add("X-Token", _options.MerchantToken);

        using var response = await httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Monobank invoice/create failed for orders [{OrderIds}]: {Status} {Body}", string.Join(", ", orderIds), response.StatusCode, body);
            throw new InvalidOperationException("Monobank invoice creation failed.");
        }

        var invoice = JsonSerializer.Deserialize<InvoiceCreateResponse>(body)
            ?? throw new InvalidOperationException("Monobank invoice/create returned an unexpected response.");

        foreach (var order in orders)
        {
            order.Payment!.Status = PaymentStatus.Pending;
            order.Payment.ProviderInvoiceId = invoice.InvoiceId;
        }
        await db.SaveChangesAsync(ct);

        return new PaymentInvoice(invoice.PageUrl);
    }

    public async Task<bool> HandleWebhookAsync(string rawBody, string? signatureHeader, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            logger.LogWarning("Monobank webhook: missing X-Sign header");
            return false;
        }

        var publicKey = await GetPublicKeyAsync(ct);
        if (publicKey is null)
        {
            logger.LogError("Monobank webhook: could not obtain merchant public key for verification");
            return false;
        }

        byte[] signatureBytes;
        try
        {
            signatureBytes = Convert.FromBase64String(signatureHeader);
        }
        catch (FormatException)
        {
            logger.LogWarning("Monobank webhook: X-Sign header is not valid base64");
            return false;
        }

        // The signature is over the exact raw body bytes — verify before any JSON parsing/
        // re-serialization, which could reorder fields and invalidate it.
        var bodyBytes = Encoding.UTF8.GetBytes(rawBody);
        bool verified;
        try
        {
            verified = publicKey.VerifyData(bodyBytes, signatureBytes, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        }
        catch (CryptographicException)
        {
            verified = false;
        }
        if (!verified)
        {
            logger.LogWarning("Monobank webhook: signature verification failed");
            return false;
        }

        InvoiceStatusPayload? status;
        try
        {
            status = JsonSerializer.Deserialize<InvoiceStatusPayload>(rawBody);
        }
        catch (JsonException)
        {
            logger.LogWarning("Monobank webhook: body is not valid JSON");
            return false;
        }
        if (status?.InvoiceId is null)
        {
            logger.LogWarning("Monobank webhook: payload missing invoiceId");
            return false;
        }

        // One invoice can cover several orders (see #377's cart checkout) — every one of them got
        // the same ProviderInvoiceId when the batch invoice was created, so all must be updated
        // together from this single webhook delivery.
        var payments = await db.Payments.Include(p => p.Order)
            .Where(p => p.ProviderInvoiceId == status.InvoiceId).ToListAsync(ct);
        if (payments.Count == 0)
        {
            logger.LogWarning("Monobank webhook: no payment found for invoiceId {InvoiceId}", status.InvoiceId);
            return false;
        }

        // Idempotent: Monobank retries webhooks until it gets a 2xx, so a repeat delivery must not
        // double-apply. All payments in a batch move together, so checking one is enough.
        if (payments[0].Status == PaymentStatus.Succeeded)
        {
            return true;
        }

        switch (status.Status)
        {
            case "success":
                foreach (var payment in payments)
                {
                    payment.Status = PaymentStatus.Succeeded;
                    payment.PaidAtUtc = DateTime.UtcNow;
                    payment.Order.SetStatus(OrderStatus.Paid);
                }
                logger.LogInformation(
                    "Monobank webhook: orders [{OrderIds}] paid via invoice {InvoiceId}",
                    string.Join(", ", payments.Select(p => p.OrderId)), status.InvoiceId);
                break;
            case "failure":
            case "expired":
            case "reversed":
                foreach (var payment in payments)
                {
                    payment.Status = PaymentStatus.Failed;
                }
                logger.LogWarning("Monobank webhook: invoice {InvoiceId} ended as {Status}", status.InvoiceId, status.Status);
                break;
            default:
                // created/processing/hold — still in flight, nothing to apply yet.
                break;
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<ECDsa?> GetPublicKeyAsync(CancellationToken ct)
    {
        if (_cachedPublicKey is not null) return _cachedPublicKey;

        await PublicKeyLock.WaitAsync(ct);
        try
        {
            if (_cachedPublicKey is not null) return _cachedPublicKey;

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/api/merchant/pubkey");
            request.Headers.Add("X-Token", _options.MerchantToken);
            using var response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Monobank pubkey fetch failed: {Status}", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<PublicKeyResponse>(cancellationToken: ct);
            if (payload?.Key is null) return null;

            var pem = Encoding.UTF8.GetString(Convert.FromBase64String(payload.Key));
            var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(pem);
            _cachedPublicKey = ecdsa;
            return _cachedPublicKey;
        }
        finally
        {
            PublicKeyLock.Release();
        }
    }

    private record InvoiceCreateResponse(
        [property: JsonPropertyName("invoiceId")] string InvoiceId,
        [property: JsonPropertyName("pageUrl")] string PageUrl);

    private record PublicKeyResponse([property: JsonPropertyName("key")] string Key);

    private record InvoiceStatusPayload(
        [property: JsonPropertyName("invoiceId")] string? InvoiceId,
        [property: JsonPropertyName("status")] string? Status);
}
