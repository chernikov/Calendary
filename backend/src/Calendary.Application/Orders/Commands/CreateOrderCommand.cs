using Calendary.Application.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

// The order isn't created until the customer actually commits a photo — no more empty "Created"-
// status rows left behind by someone who clicked "Створити календар" and then closed the tab.
// Combines the old bare Create + UploadPhoto into one call (see #348).
public record CreateOrderCommand(Guid UserId, byte[] PhotoBytes, string ContentType) : IRequest<Order>;

public class CreateOrderCommandHandler(
    IAppDbContext db,
    IFileStorage fileStorage,
    IPhotoThumbnailGenerator thumbnailGenerator,
    IAppSettingsService appSettings) : IRequestHandler<CreateOrderCommand, Order>
{
    public async Task<Order> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var url = await fileStorage.SaveAsync(request.PhotoBytes, request.ContentType, "photos", ct);
        var thumb = thumbnailGenerator.Generate(new StoredFile(request.PhotoBytes, request.ContentType));
        var thumbUrl = await fileStorage.SaveAsync(thumb.Content, thumb.ContentType, "photo-thumbs", ct);

        var order = new Order { UserId = request.UserId, Price = await appSettings.GetBasePriceAsync(ct) };
        order.Photos.Add(new OrderPhoto { OrderId = order.Id, Url = url, ThumbUrl = thumbUrl });
        order.SetStatus(OrderStatus.PhotoUploaded);
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        return (await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, order.Id, ct))!;
    }
}
