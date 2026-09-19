using Calendary.Api.Auth;
using Calendary.Api.Dtos;
using Calendary.Api.Photos;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(
    AppDbContext db,
    IImageGenerationService generationService,
    IPaymentService paymentService,
    ICalendarPdfService pdfService,
    IFileStorage fileStorage,
    ILogger<OrdersController> logger) : ControllerBase
{
    private const int MaxLabelLength = 22;
    private const int MaxPhotosPerOrder = 20; // abuse safeguard only — not a product-facing cap

    private async Task<Order?> LoadOwnedOrderAsync(Guid orderId)
    {
        var userId = User.GetUserId();
        return await db.Orders
            .Include(o => o.Photos)
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets).ThenInclude(s => s.Prompt)
            .Include(o => o.Sheets).ThenInclude(s => s.ImageStyle)
            .Include(o => o.Sheets).ThenInclude(s => s.PinnedPhoto)
            .Include(o => o.Sheets).ThenInclude(s => s.Variants)
            .Include(o => o.Payment)
            .Include(o => o.Delivery)
            // Photos, Sheets and PersonalDates are sibling collections on the same query — a
            // single join multiplies rows. AsSplitQuery issues one query per collection.
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
    }

    private static bool IsValidPhotoId(Order order, Guid? photoId) =>
        photoId is null || order.Photos.Any(p => p.Id == photoId);

    // Expiry only matters while the order is still moving through the funnel — once payment is
    // captured it's fulfillment's problem, not a reason to block anything. Keep in sync with
    // OrderExpiryBackgroundService's exemption list (auto-archives expired orders on the same rule).
    private static readonly OrderStatus[] ExemptFromExpiry =
        [OrderStatus.Paid, OrderStatus.Printing, OrderStatus.Shipped, OrderStatus.Delivered];

    private static bool IsExpired(Order order) =>
        !ExemptFromExpiry.Contains(order.Status) && DateTime.UtcNow > order.ExpiresAtUtc;

    // The order isn't created until the customer actually commits a photo — no more empty
    // "Created"-status rows left behind by someone who clicked "Створити календар" and then
    // closed the tab. Combines the old bare Create + UploadPhoto into one call (see #348).
    [HttpPost]
    [RequestSizeLimit(PhotoIntake.MaxBytes + 64 * 1024)]
    public async Task<ActionResult<OrderDto>> Create([FromForm] IFormFile? photo, CancellationToken ct)
    {
        var intake = await PhotoIntake.ReadAsync(photo, ct);
        if (!intake.Ok)
        {
            return BadRequest(new { error = intake.Error });
        }

        var url = await fileStorage.SaveAsync(intake.Bytes, intake.ContentType, "photos", ct);
        var thumb = PhotoThumbnailGenerator.Generate(new StoredFile(intake.Bytes, intake.ContentType));
        var thumbUrl = await fileStorage.SaveAsync(thumb.Content, thumb.ContentType, "photo-thumbs", ct);

        var order = new Order { UserId = User.GetUserId() };
        order.Photos.Add(new OrderPhoto { OrderId = order.Id, Url = url, ThumbUrl = thumbUrl });
        order.SetStatus(OrderStatus.PhotoUploaded);
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        order = await LoadOwnedOrderAsync(order.Id);
        return Ok(order!.ToDto());
    }

    // Uploads are sequential, one file per request — the first (Create, above) creates the order;
    // this adds any further photo to it. Kept as a separate small endpoint (rather than accepting
    // a file list on Create) so the request body size never grows with photo count.
    [HttpPost("{orderId:guid}/photos")]
    [RequestSizeLimit(PhotoIntake.MaxBytes + 64 * 1024)]
    public async Task<ActionResult<OrderDto>> AddPhoto(Guid orderId, [FromForm] IFormFile? photo, CancellationToken ct)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (IsExpired(order)) return Conflict("Order has expired.");
        if (order.Status is not (OrderStatus.PhotoUploaded or OrderStatus.DetailsSubmitted))
        {
            return Conflict("Photos can only be added before generation starts.");
        }
        if (order.Photos.Count >= MaxPhotosPerOrder)
        {
            return Conflict("Too many photos.");
        }

        var intake = await PhotoIntake.ReadAsync(photo, ct);
        if (!intake.Ok)
        {
            return BadRequest(new { error = intake.Error });
        }

        var url = await fileStorage.SaveAsync(intake.Bytes, intake.ContentType, "photos", ct);
        var thumb = PhotoThumbnailGenerator.Generate(new StoredFile(intake.Bytes, intake.ContentType));
        var thumbUrl = await fileStorage.SaveAsync(thumb.Content, thumb.ContentType, "photo-thumbs", ct);

        db.OrderPhotos.Add(new OrderPhoto { OrderId = order.Id, Url = url, ThumbUrl = thumbUrl });
        await db.SaveChangesAsync(ct);

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpDelete("{orderId:guid}/photos/{photoId:guid}")]
    public async Task<ActionResult<OrderDto>> RemovePhoto(Guid orderId, Guid photoId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (order.Status is not (OrderStatus.PhotoUploaded or OrderStatus.DetailsSubmitted))
        {
            return Conflict("Photos can only be changed before generation starts.");
        }

        var photo = order.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo is null) return NotFound();
        if (order.Photos.Count == 1) return Conflict("At least one photo is required.");

        db.OrderPhotos.Remove(photo);
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid orderId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> List()
    {
        var userId = User.GetUserId();
        var orders = await db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.Status.ToString(),
                o.Price,
                o.CreatedAtUtc,
                o.StatusUpdatedAtUtc,
                o.Sheets.Where(s => s.Kind == SheetKind.Cover && s.Prompt != null).Select(s => s.Prompt!.Name).FirstOrDefault(),
                o.Sheets.Where(s => s.Kind == SheetKind.Cover).Select(s => s.ImageUrl).FirstOrDefault(),
                o.IsArchived))
            .ToListAsync();

        return Ok(orders);
    }

    /// Saves the user's per-sheet picks (prompt + image style for the cover and each month),
    /// creating or updating the 13 Sheet rows before generation starts.
    [HttpPut("{orderId:guid}/sheet-plan")]
    public async Task<ActionResult<OrderDto>> SaveSheetPlan(Guid orderId, SaveSheetPlanRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (IsExpired(order)) return Conflict("Order has expired.");
        if (order.Status is not (OrderStatus.PhotoUploaded or OrderStatus.DetailsSubmitted))
        {
            return Conflict("The sheet plan can only be changed before generation starts.");
        }

        var items = request.Items ?? [];
        if (items.Count != 13 || items.Select(i => i.Index).Distinct().Count() != 13 ||
            items.Any(i => i.Index is < 0 or > 12))
        {
            return BadRequest("The plan must contain exactly 13 items with indexes 0 (cover) through 12.");
        }

        var promptIds = items.Select(i => i.PromptId).Distinct().ToList();
        var styleIds = items.Select(i => i.ImageStyleId).Distinct().ToList();
        var knownPrompts = await db.Prompts.Where(p => promptIds.Contains(p.Id)).Select(p => p.Id).ToListAsync();
        var knownStyles = await db.ImageStyles.Where(s => styleIds.Contains(s.Id)).Select(s => s.Id).ToListAsync();
        if (knownPrompts.Count != promptIds.Count) return BadRequest("Unknown prompt.");
        if (knownStyles.Count != styleIds.Count) return BadRequest("Unknown image style.");
        if (items.Any(i => !IsValidPhotoId(order, i.PhotoId))) return BadRequest("Unknown photo.");

        foreach (var item in items.OrderBy(i => i.Index))
        {
            var sheet = order.Sheets.FirstOrDefault(s => s.Index == item.Index);
            if (sheet is null)
            {
                sheet = new Sheet
                {
                    OrderId = order.Id,
                    Kind = item.Index == 0 ? SheetKind.Cover : SheetKind.Month,
                    Index = item.Index
                };
                db.Sheets.Add(sheet);
            }
            sheet.PromptId = item.PromptId;
            sheet.ImageStyleId = item.ImageStyleId;
            sheet.PinnedPhotoId = item.PhotoId;
        }

        order.SetStatus(OrderStatus.DetailsSubmitted);
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("{orderId:guid}/dates")]
    public async Task<ActionResult<OrderDto>> AddDate(Guid orderId, AddPersonalDateRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Label) || request.Label.Length > MaxLabelLength)
        {
            return BadRequest($"Label must be 1-{MaxLabelLength} characters.");
        }
        if (request.Month is < 1 or > 12 || request.Day < 1 || request.Day > DateTime.DaysInMonth(2024, request.Month))
        {
            return BadRequest("Invalid day/month.");
        }

        db.PersonalDates.Add(new PersonalDate
        {
            OrderId = order.Id,
            Day = request.Day,
            Month = request.Month,
            Label = request.Label.Trim()
        });
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpDelete("{orderId:guid}/dates/{dateId:guid}")]
    public async Task<ActionResult<OrderDto>> RemoveDate(Guid orderId, Guid dateId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        var date = order.PersonalDates.FirstOrDefault(d => d.Id == dateId);
        if (date is null) return NotFound();

        db.PersonalDates.Remove(date);
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    /// The single customer-facing generation trigger (see #351) — used by the sheet picker modal
    /// on every page (planning-step tiles, cover, month). A sheet's first-ever variant is free,
    /// matching the old planning-step behavior; any variant after that costs a regeneration, same
    /// budget the old dedicated "Перегенерувати" buttons used to spend. Creating a fresh variant
    /// never touches an already-generated one — see ActivateVariant to switch back to an older one.
    [HttpPost("{orderId:guid}/sheets/{index:int}/generate")]
    public async Task<ActionResult<OrderDto>> GenerateSheet(Guid orderId, int index, GenerateSheetRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (IsExpired(order)) return Conflict("Order has expired.");
        if (index is < 0 or > 12) return BadRequest("Index must be 0 (cover) through 12.");
        if (order.Status is OrderStatus.Paid or OrderStatus.Printing or OrderStatus.Shipped
            or OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return Conflict("Order is no longer editable.");
        }
        if (order.Photos.Count == 0) return Conflict("Upload a photo first.");
        if (!IsValidPhotoId(order, request.PhotoId)) return BadRequest("Unknown photo.");

        if (await db.Prompts.FindAsync(request.PromptId) is null) return BadRequest("Unknown prompt.");
        if (await db.ImageStyles.FindAsync(request.ImageStyleId) is null) return BadRequest("Unknown image style.");

        var sheet = order.Sheets.FirstOrDefault(s => s.Index == index);
        var isAnotherVariant = sheet is { Status: SheetStatus.Ready };
        if (sheet is null)
        {
            sheet = new Sheet
            {
                OrderId = order.Id,
                Kind = index == 0 ? SheetKind.Cover : SheetKind.Month,
                Index = index
            };
            db.Sheets.Add(sheet);
        }
        else if (sheet.Status == SheetStatus.Generating)
        {
            return Conflict("This sheet is already generating.");
        }

        if (isAnotherVariant && order.RegenerationsRemaining <= 0)
        {
            return Conflict("No regenerations remaining.");
        }

        sheet.PromptId = request.PromptId;
        sheet.ImageStyleId = request.ImageStyleId;
        sheet.PinnedPhotoId = request.PhotoId;
        if (isAnotherVariant)
        {
            order.RegenerationsRemaining -= 1;
        }
        await db.SaveChangesAsync();

        await generationService.GenerateSheetPreviewAsync(orderId, sheet.Id);

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    /// Restores a previously generated variant as the sheet's active image — no generation, no
    /// regeneration budget cost, since nothing new is produced (see #351's gallery navigation).
    [HttpPost("{orderId:guid}/sheets/{sheetId:guid}/variants/{variantId:guid}/activate")]
    public async Task<ActionResult<OrderDto>> ActivateVariant(Guid orderId, Guid sheetId, Guid variantId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (order.Status is OrderStatus.Paid or OrderStatus.Printing or OrderStatus.Shipped
            or OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return Conflict("Order is no longer editable.");
        }

        var sheet = order.Sheets.FirstOrDefault(s => s.Id == sheetId);
        if (sheet is null) return NotFound();

        var variant = sheet.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null) return NotFound();

        sheet.ActiveVariantId = variant.Id;
        sheet.ImageUrl = variant.ImageUrl;
        sheet.Status = SheetStatus.Ready;
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("{orderId:guid}/generate")]
    public async Task<ActionResult<OrderDto>> Generate(Guid orderId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (IsExpired(order)) return Conflict("Order has expired.");
        if (order.Sheets.Count != 13 || order.Sheets.Any(s => s.PromptId is null || s.ImageStyleId is null))
        {
            return BadRequest("Complete the sheet plan (prompt and style for every sheet) before generating.");
        }

        await generationService.StartOrderGenerationAsync(orderId);

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    // Dev/demo-only: flips a sheet into the Failed state so the frontend's failure/retry
    // UI can be exercised without waiting for a (nonexistent) real generation failure.
    [HttpPost("{orderId:guid}/sheets/{sheetId:guid}/simulate-failure")]
    public async Task<ActionResult<OrderDto>> SimulateFailure(Guid orderId, Guid sheetId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        var sheet = order.Sheets.FirstOrDefault(s => s.Id == sheetId);
        if (sheet is null) return NotFound();

        sheet.Status = SheetStatus.Failed;
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("{orderId:guid}/cover/confirm")]
    public async Task<ActionResult<OrderDto>> ConfirmCover(Guid orderId, ConfirmCoverRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        var cover = order.Sheets.FirstOrDefault(s => s.Id == request.SheetId && s.Kind == SheetKind.Cover);
        if (cover is null) return BadRequest("Not a cover sheet.");
        if (cover.Status != SheetStatus.Ready) return Conflict("Cover is not ready yet.");

        cover.IsSelected = true;
        if (order.Status is OrderStatus.CoverReady or OrderStatus.Generating)
        {
            order.SetStatus(OrderStatus.CoverConfirmed);
        }
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("{orderId:guid}/checkout")]
    public async Task<ActionResult<OrderDto>> Checkout(Guid orderId, CheckoutRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (IsExpired(order)) return Conflict("Order has expired.");

        if (order.Delivery is null)
        {
            order.Delivery = new Delivery { OrderId = order.Id };
            db.Deliveries.Add(order.Delivery);
        }
        order.Delivery.RecipientName = request.RecipientName;
        order.Delivery.Phone = request.Phone;
        order.Delivery.City = request.City;
        order.Delivery.WarehouseNumber = request.WarehouseNumber;
        order.Delivery.WarehouseAddress = request.WarehouseAddress;

        order.SetStatus(OrderStatus.AwaitingPayment);
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("{orderId:guid}/pay")]
    public async Task<ActionResult<OrderDto>> Pay(Guid orderId, PayRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (IsExpired(order)) return Conflict("Order has expired.");

        // Idempotency: a retried/duplicate POST (double-click, frontend retry, provider webhook
        // racing the synchronous response) must not charge a second time. Retrying after a
        // *failed* payment is still allowed — only a prior success short-circuits.
        if (order.Status == OrderStatus.Paid || order.Payment?.Status == PaymentStatus.Succeeded)
        {
            logger.LogInformation("Pay: order {OrderId} is already paid, skipping charge", orderId);
            return Ok(order.ToDto());
        }

        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var method))
        {
            return BadRequest("Unknown payment method.");
        }

        var result = await paymentService.ChargeAsync(orderId, method, order.Price);

        if (order.Payment is null)
        {
            order.Payment = new Payment { OrderId = order.Id };
            db.Payments.Add(order.Payment);
        }
        order.Payment.Method = method;
        order.Payment.Amount = order.Price;

        if (result.Succeeded)
        {
            order.Payment.Status = PaymentStatus.Succeeded;
            order.Payment.PaidAtUtc = DateTime.UtcNow;
            order.SetStatus(OrderStatus.Paid);
            logger.LogInformation(
                "Pay: order {OrderId} charged successfully via {Method}, amount {Amount}", orderId, method, order.Price);
        }
        else
        {
            order.Payment.Status = PaymentStatus.Failed;
            logger.LogWarning(
                "Pay: order {OrderId} charge failed via {Method}: {Reason}", orderId, method, result.FailureReason ?? "unknown");
        }

        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(orderId);
        return result.Succeeded ? Ok(order!.ToDto()) : StatusCode(402, order!.ToDto());
    }

    [HttpGet("{orderId:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid orderId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        var sheetsReady = order.Sheets.Count == 13 && order.Sheets.All(s => s.Status == SheetStatus.Ready);
        if (!sheetsReady) return Conflict("Calendar is not fully generated yet.");

        // Before payment, this doubles as the customer-facing preview — stamp it so it can't pass
        // as the final print file.
        var watermark = order.Status != OrderStatus.Paid;
        var pdfBytes = await pdfService.GenerateAsync(orderId, watermark);
        return File(pdfBytes, "application/pdf", $"calendary-{orderId}.pdf");
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid orderId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (order.Status is OrderStatus.Paid or OrderStatus.Printing or OrderStatus.Shipped or OrderStatus.Delivered)
        {
            return Conflict("Order has already been paid; cancellation would require a refund flow that is out of scope for this demo.");
        }

        order.SetStatus(OrderStatus.Cancelled);
        await db.SaveChangesAsync();
        return Ok(order.ToDto());
    }

    // Archiving is purely a list-visibility flag — orthogonal to the OrderStatus state machine,
    // so it's set directly rather than via SetStatus().
    [HttpPost("{orderId:guid}/archive")]
    public async Task<IActionResult> Archive(Guid orderId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        order.IsArchived = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{orderId:guid}/unarchive")]
    public async Task<IActionResult> Unarchive(Guid orderId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        order.IsArchived = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
