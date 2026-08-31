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
            .Include(o => o.StyleCategory)
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets)
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

    [HttpPost("{orderId:guid}/photo")]
    public async Task<ActionResult<OrderDto>> UploadPhoto(Guid orderId, UploadPhotoRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        if (!PhotoIntake.TryDecode(request.PhotoDataUrl, out var bytes, out var contentType, out var error))
        {
            return BadRequest(error);
        }

        order.PhotoUrl = await fileStorage.SaveAsync(bytes, contentType, "photos");
        order.SetStatus(OrderStatus.PhotoUploaded);
        await db.SaveChangesAsync();
        return Ok(order.ToDto());
    }

    [HttpPost("{orderId:guid}/style")]
    public async Task<ActionResult<OrderDto>> SelectStyle(Guid orderId, SelectStyleRequest request)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

        var category = await db.StyleCategories.FindAsync(request.StyleCategoryId);
        if (category is null) return BadRequest("Unknown style category.");

        order.StyleCategoryId = category.Id;
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

    [HttpPost("{orderId:guid}/generate")]
    public async Task<ActionResult<OrderDto>> Generate(Guid orderId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();
        if (order.StyleCategoryId is null) return BadRequest("Select a style before generating.");

        await generationService.StartOrderGenerationAsync(orderId);

        order = await LoadOwnedOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("{orderId:guid}/sheets/{sheetId:guid}/regenerate")]
    public async Task<ActionResult<OrderDto>> RegenerateSheet(Guid orderId, Guid sheetId)
    {
        var order = await LoadOwnedOrderAsync(orderId);
        if (order is null) return NotFound();

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
