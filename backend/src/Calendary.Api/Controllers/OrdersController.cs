using Calendary.Api.Auth;
using Calendary.Api.Dtos;
using Calendary.Api.Filters;
using Calendary.Api.Photos;
using Calendary.Application.Orders;
using Calendary.Application.Orders.Commands;
using Calendary.Application.Orders.Queries;
using Calendary.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Calendary.Api.Controllers;

// Thin by design (see #298): every action maps a request DTO to a Command/Query, sends it via
// MediatR, and maps the result back to a DTO. Business logic, order-ownership loading, and
// validation all live in Calendary.Application/Orders — this controller has no AppDbContext, no
// business rules. AppOperationExceptionFilter turns an AppOperationException thrown by a
// handler into the same response shape the old inline BadRequest/Conflict calls used to produce.
[ApiController]
[Route("api/orders")]
[Authorize]
[TypeFilter(typeof(AppOperationExceptionFilter))]
public class OrdersController(ISender sender, IOptions<MonobankOptions> monobankOptions) : ControllerBase
{
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

        var order = await sender.Send(new CreateOrderCommand(User.GetUserId(), intake.Bytes, intake.ContentType), ct);
        return Ok(order.ToDto());
    }

    // Uploads are sequential, one file per request — the first (Create, above) creates the order;
    // this adds any further photo to it. Kept as a separate small endpoint (rather than accepting
    // a file list on Create) so the request body size never grows with photo count.
    [HttpPost("{orderId:guid}/photos")]
    [RequestSizeLimit(PhotoIntake.MaxBytes + 64 * 1024)]
    public async Task<ActionResult<OrderDto>> AddPhoto(Guid orderId, [FromForm] IFormFile? photo, CancellationToken ct)
    {
        var intake = await PhotoIntake.ReadAsync(photo, ct);
        if (!intake.Ok)
        {
            return BadRequest(new { error = intake.Error });
        }

        var order = await sender.Send(new AddPhotoCommand(User.GetUserId(), orderId, intake.Bytes, intake.ContentType), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpDelete("{orderId:guid}/photos/{photoId:guid}")]
    public async Task<ActionResult<OrderDto>> RemovePhoto(Guid orderId, Guid photoId, CancellationToken ct)
    {
        var order = await sender.Send(new RemovePhotoCommand(User.GetUserId(), orderId, photoId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid orderId, CancellationToken ct)
    {
        var order = await sender.Send(new GetOwnedOrderQuery(User.GetUserId(), orderId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    // Lightweight sibling of Get (#303) — only status + per-sheet status/kind, no ImageUrl/prompt/
    // style/variant payload, for the "generating"/"month" pages' ~1.5s poll to hit instead of the
    // full order graph on every tick.
    [HttpGet("{orderId:guid}/progress")]
    public async Task<ActionResult<OrderProgressDto>> GetProgress(Guid orderId, CancellationToken ct)
    {
        var progress = await sender.Send(new GetOwnedOrderProgressQuery(User.GetUserId(), orderId), ct);
        return progress is null ? NotFound() : Ok(progress.ToDto());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> List(CancellationToken ct)
    {
        var summaries = await sender.Send(new ListOrderSummariesQuery(User.GetUserId()), ct);
        var dtos = summaries.Select(s => new OrderSummaryDto(
            s.Id, s.Status, s.Price, s.PrintQuantity, s.CreatedAtUtc, s.StatusUpdatedAtUtc,
            s.StyleName, s.CoverImageUrl, s.IsArchived)).ToList();
        return Ok(dtos);
    }

    /// Saves the user's per-sheet picks (prompt + image style for the cover and each month),
    /// creating or updating the 13 Sheet rows before generation starts.
    [HttpPut("{orderId:guid}/sheet-plan")]
    public async Task<ActionResult<OrderDto>> SaveSheetPlan(Guid orderId, SaveSheetPlanRequest request, CancellationToken ct)
    {
        var items = (request.Items ?? []).Select(i => new SheetPlanEntry(i.Index, i.PromptId, i.ImageStyleId, i.PhotoId)).ToList();
        var order = await sender.Send(new SaveSheetPlanCommand(User.GetUserId(), orderId, items), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("{orderId:guid}/dates")]
    public async Task<ActionResult<OrderDto>> AddDate(Guid orderId, AddPersonalDateRequest request, CancellationToken ct)
    {
        var order = await sender.Send(new AddPersonalDateCommand(User.GetUserId(), orderId, request.Day, request.Month, request.Label), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpDelete("{orderId:guid}/dates/{dateId:guid}")]
    public async Task<ActionResult<OrderDto>> RemoveDate(Guid orderId, Guid dateId, CancellationToken ct)
    {
        var order = await sender.Send(new RemovePersonalDateCommand(User.GetUserId(), orderId, dateId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    /// Which countries' public holidays to mark in the printed calendar, and which weekday the
    /// grid starts on (see #364) — set on the same personal-dates step, saved immediately on
    /// every change, same as AddDate/RemoveDate above.
    [HttpPut("{orderId:guid}/holiday-settings")]
    public async Task<ActionResult<OrderDto>> SaveHolidaySettings(Guid orderId, SaveHolidaySettingsRequest request, CancellationToken ct)
    {
        var order = await sender.Send(new SaveHolidaySettingsCommand(User.GetUserId(), orderId, request.Countries, request.WeekStart), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    /// The single customer-facing generation trigger (see #351) — used by the sheet picker modal
    /// on every page (planning-step tiles, cover, month). A sheet's first-ever variant is free,
    /// matching the old planning-step behavior; any variant after that costs a regeneration, same
    /// budget the old dedicated "Перегенерувати" buttons used to spend. Creating a fresh variant
    /// never touches an already-generated one — see ActivateVariant to switch back to an older one.
    [HttpPost("{orderId:guid}/sheets/{index:int}/generate")]
    public async Task<ActionResult<OrderDto>> GenerateSheet(Guid orderId, int index, GenerateSheetRequest request, CancellationToken ct)
    {
        var order = await sender.Send(new GenerateSheetCommand(User.GetUserId(), orderId, index, request.PromptId, request.ImageStyleId, request.PhotoId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    /// Restores a previously generated variant as the sheet's active image — no generation, no
    /// regeneration budget cost, since nothing new is produced (see #351's gallery navigation).
    [HttpPost("{orderId:guid}/sheets/{sheetId:guid}/variants/{variantId:guid}/activate")]
    public async Task<ActionResult<OrderDto>> ActivateVariant(Guid orderId, Guid sheetId, Guid variantId, CancellationToken ct)
    {
        var order = await sender.Send(new ActivateVariantCommand(User.GetUserId(), orderId, sheetId, variantId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("{orderId:guid}/generate")]
    public async Task<ActionResult<OrderDto>> Generate(Guid orderId, CancellationToken ct)
    {
        var order = await sender.Send(new StartGenerationCommand(User.GetUserId(), orderId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    // Dev/demo-only: flips a sheet into the Failed state so the frontend's failure/retry UI can
    // be exercised without waiting for a (nonexistent) real generation failure. Never real in
    // production — 404s there rather than shipping a way for any order owner to fail their own
    // sheets (see #323).
    [HttpPost("{orderId:guid}/sheets/{sheetId:guid}/simulate-failure")]
    public async Task<ActionResult<OrderDto>> SimulateFailure(Guid orderId, Guid sheetId, CancellationToken ct)
    {
        var order = await sender.Send(new SimulateFailureCommand(User.GetUserId(), orderId, sheetId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("{orderId:guid}/cover/confirm")]
    public async Task<ActionResult<OrderDto>> ConfirmCover(Guid orderId, ConfirmCoverRequest request, CancellationToken ct)
    {
        var order = await sender.Send(new ConfirmCoverCommand(User.GetUserId(), orderId, request.SheetId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("{orderId:guid}/checkout")]
    public async Task<ActionResult<OrderDto>> Checkout(Guid orderId, CheckoutRequest request, CancellationToken ct)
    {
        var delivery = new DeliveryInfo(request.RecipientName, request.Phone, request.City, request.WarehouseNumber, request.WarehouseAddress);
        var order = await sender.Send(new CheckoutCommand(User.GetUserId(), orderId, delivery), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    /// A customer-entered discount code applied at checkout (see #393) — one code per order.
    [HttpPost("{orderId:guid}/promo-code")]
    public async Task<ActionResult<OrderDto>> ApplyPromoCode(Guid orderId, ApplyPromoCodeRequest request, CancellationToken ct)
    {
        var order = await sender.Send(new ApplyPromoCodeCommand(User.GetUserId(), orderId, request.Code), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpDelete("{orderId:guid}/promo-code")]
    public async Task<ActionResult<OrderDto>> RemovePromoCode(Guid orderId, CancellationToken ct)
    {
        var order = await sender.Send(new RemovePromoCodeCommand(User.GetUserId(), orderId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("{orderId:guid}/pay")]
    public async Task<ActionResult<PayResponseDto>> Pay(Guid orderId, CancellationToken ct)
    {
        var invoice = await sender.Send(new PayCommand(User.GetUserId(), orderId, monobankOptions.Value.PublicBaseUrl), ct);
        return Ok(new PayResponseDto(invoice.PageUrl));
    }

    [HttpPut("{orderId:guid}/print-quantity")]
    public async Task<ActionResult<OrderDto>> SetPrintQuantity(Guid orderId, SetPrintQuantityRequest request, CancellationToken ct)
    {
        var order = await sender.Send(new SetPrintQuantityCommand(User.GetUserId(), orderId, request.Quantity), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("checkout-batch")]
    public async Task<IActionResult> CheckoutBatch(BatchCheckoutRequest request, CancellationToken ct)
    {
        var delivery = new DeliveryInfo(request.RecipientName, request.Phone, request.City, request.WarehouseNumber, request.WarehouseAddress);
        await sender.Send(new CheckoutBatchCommand(User.GetUserId(), request.OrderIds, delivery), ct);
        return Ok();
    }

    [HttpPost("pay-batch")]
    public async Task<ActionResult<PayResponseDto>> PayBatch([FromBody] IReadOnlyList<Guid> orderIds, CancellationToken ct)
    {
        var invoice = await sender.Send(new PayBatchCommand(User.GetUserId(), orderIds, monobankOptions.Value.PublicBaseUrl), ct);
        return Ok(new PayResponseDto(invoice.PageUrl));
    }

    [HttpGet("{orderId:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid orderId, CancellationToken ct)
    {
        var pdfBytes = await sender.Send(new GenerateOrderPdfQuery(User.GetUserId(), orderId), ct);
        return pdfBytes is null ? NotFound() : File(pdfBytes, "application/pdf", $"calendary-{orderId}.pdf");
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid orderId, CancellationToken ct)
    {
        var order = await sender.Send(new CancelOrderCommand(User.GetUserId(), orderId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    // Archiving is purely a list-visibility flag — orthogonal to the OrderStatus state machine.
    [HttpPost("{orderId:guid}/archive")]
    public async Task<IActionResult> Archive(Guid orderId, CancellationToken ct)
    {
        var found = await sender.Send(new ArchiveOrderCommand(User.GetUserId(), orderId, true), ct);
        return found ? NoContent() : NotFound();
    }

    [HttpPost("{orderId:guid}/unarchive")]
    public async Task<IActionResult> Unarchive(Guid orderId, CancellationToken ct)
    {
        var found = await sender.Send(new ArchiveOrderCommand(User.GetUserId(), orderId, false), ct);
        return found ? NoContent() : NotFound();
    }
}
