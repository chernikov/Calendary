using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record RemovePhotoCommand(Guid UserId, Guid OrderId, Guid PhotoId) : IRequest<Order?>;

public class RemovePhotoCommandHandler(IAppDbContext db) : IRequestHandler<RemovePhotoCommand, Order?>
{
    public async Task<Order?> Handle(RemovePhotoCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (order.Status is not (OrderStatus.PhotoUploaded or OrderStatus.DetailsSubmitted))
        {
            throw new AppOperationException("Photos can only be changed before generation starts.", 409);
        }

        var photo = order.Photos.FirstOrDefault(p => p.Id == request.PhotoId);
        if (photo is null) return null;
        if (order.Photos.Count == 1) throw new AppOperationException("At least one photo is required.", 409);

        db.OrderPhotos.Remove(photo);
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
