using Calendary.Api.Auth;
using Calendary.Api.Dtos;
using Calendary.Api.Photos;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
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
    IFileStorage fileStorage) : ControllerBase
{
    private const int MaxLabelLength = 22;

    private async Task<Order?> LoadOwnedOrderAsync(Guid orderId)
    {
        var userId = User.GetUserId();
        return await db.Orders
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets).ThenInclude(s => s.Prompt)
            .Include(o => o.Sheets).ThenInclude(s => s.ImageStyle)
            .Include(o => o.Payment)
            .Include(o => o.Delivery)
            // Sheets and PersonalDates are sibling collections on the same query — a single join
            // multiplies rows (sheets × dates). AsSplitQuery issues one query per collection.
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create()
    {
        var order = new Order { UserId = User.GetUserId() };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        order = await LoadOwnedOrderAsync(order.Id);
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
                o.Sheets.Where(s => s.Kind == SheetKind.Cover).Select(s => s.ImageUrl).FirstOrDefault()))
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost("{orderId:guid}/photo")]
    [RequestSizeLimit(PhotoIntake.MaxBytes + 64 * 1024)]
    public async Task<ActionResult<OrderDto>> UploadPhoto(Guid orderId, [FromForm] IFormFile? photo, CancellationToken ct)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        var intake = await PhotoIntake.ReadAsync(photo, ct);
        if (!intake.Ok)
        {
            return BadRequest(new { error = intake.Error });
        }

        order.PhotoUrl = await fileStorage.SaveAsync(intake.Bytes, intake.ContentType, "photos", ct);
        order.SetStatus(OrderStatus.PhotoUploaded);
        await db.SaveChangesAsync(ct);
        return Ok(order.ToDto());
    }

    /// Saves the user's per-sheet picks (prompt + image style for the cover and each month),
    /// creating or updating the 13 Sheet rows before generation starts.
    [HttpPut("{orderId:guid}/sheet-plan")]
    public async Task<ActionResult<OrderDto>> SaveSheetPlan(Guid orderId, SaveSheetPlanRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
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

    /// Generates one sheet during the planning step (step 3): saves the picked prompt/style on
    /// the sheet (creating it if needed) and kicks off generation of a single image. Can be
    /// called repeatedly to re-generate with new picks; does not use the regeneration budget.
    [HttpPost("{orderId:guid}/sheets/{index:int}/generate")]
    public async Task<ActionResult<OrderDto>> GenerateSheet(Guid orderId, int index, GenerateSheetRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (index is < 0 or > 12) return BadRequest("Index must be 0 (cover) through 12.");
        if (order.Status is not (OrderStatus.PhotoUploaded or OrderStatus.DetailsSubmitted))
        {
            return Conflict("Sheets can only be generated one-by-one before full generation starts.");
        }
        if (order.PhotoUrl is null) return Conflict("Upload a photo first.");

        if (await db.Prompts.FindAsync(request.PromptId) is null) return BadRequest("Unknown prompt.");
        if (await db.ImageStyles.FindAsync(request.ImageStyleId) is null) return BadRequest("Unknown image style.");

        var sheet = order.Sheets.FirstOrDefault(s => s.Index == index);
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
        sheet.PromptId = request.PromptId;
        sheet.ImageStyleId = request.ImageStyleId;
        await db.SaveChangesAsync();

        await generationService.GenerateSheetPreviewAsync(orderId, sheet.Id);

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("{orderId:guid}/generate")]
    public async Task<ActionResult<OrderDto>> Generate(Guid orderId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (order.Sheets.Count != 13 || order.Sheets.Any(s => s.PromptId is null || s.ImageStyleId is null))
        {
            return BadRequest("Complete the sheet plan (prompt and style for every sheet) before generating.");
        }

        await generationService.StartOrderGenerationAsync(orderId);

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("{orderId:guid}/sheets/{sheetId:guid}/regenerate")]
    public async Task<ActionResult<OrderDto>> RegenerateSheet(Guid orderId, Guid sheetId, RegenerateSheetRequest? request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        // The user may swap the sheet's prompt/style before regenerating.
        var sheet = order.Sheets.FirstOrDefault(s => s.Id == sheetId);
        if (sheet is null) return NotFound();
        if (request?.PromptId is Guid promptId)
        {
            if (await db.Prompts.FindAsync(promptId) is null) return BadRequest("Unknown prompt.");
            sheet.PromptId = promptId;
        }
        if (request?.ImageStyleId is Guid imageStyleId)
        {
            if (await db.ImageStyles.FindAsync(imageStyleId) is null) return BadRequest("Unknown image style.");
            sheet.ImageStyleId = imageStyleId;
        }
        await db.SaveChangesAsync();

        var ok = await generationService.RegenerateSheetAsync(orderId, sheetId);
        if (!ok) return Conflict("No regenerations remaining.");

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
        }
        else
        {
            order.Payment.Status = PaymentStatus.Failed;
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

        var pdfBytes = await pdfService.GenerateAsync(orderId);
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
}
