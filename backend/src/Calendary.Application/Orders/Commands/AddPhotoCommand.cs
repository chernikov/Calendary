using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

// Uploads are sequential, one file per request — the first (CreateOrderCommand) creates the
// order; this adds any further photo to it. Kept separate (rather than accepting a file list on
// create) so the request body size never grows with photo count.
public record AddPhotoCommand(Guid UserId, Guid OrderId, byte[] PhotoBytes, string ContentType) : IRequest<Order?>;

public class AddPhotoCommandHandler(
    IAppDbContext db,
    IFileStorage fileStorage,
    IPhotoThumbnailGenerator thumbnailGenerator) : IRequestHandler<AddPhotoCommand, Order?>
{
    public async Task<Order?> Handle(AddPhotoCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (OrderAccess.IsExpired(order)) throw new AppOperationException("Order has expired.", 409);
        if (order.Status is not (OrderStatus.PhotoUploaded or OrderStatus.DetailsSubmitted))
        {
            throw new AppOperationException("Photos can only be added before generation starts.", 409);
        }
        if (order.Photos.Count >= OrderAccess.MaxPhotosPerOrder)
        {
            throw new AppOperationException("Too many photos.", 409);
        }

        var url = await fileStorage.SaveAsync(request.PhotoBytes, request.ContentType, "photos", ct);
        var thumb = thumbnailGenerator.Generate(new StoredFile(request.PhotoBytes, request.ContentType));
        var thumbUrl = await fileStorage.SaveAsync(thumb.Content, thumb.ContentType, "photo-thumbs", ct);

        db.OrderPhotos.Add(new OrderPhoto { OrderId = order.Id, Url = url, ThumbUrl = thumbUrl });
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
