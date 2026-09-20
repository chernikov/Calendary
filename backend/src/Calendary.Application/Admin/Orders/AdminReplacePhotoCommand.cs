using Calendary.Application.Common;
using Calendary.Application.Orders;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Admin.Orders;

public record AdminReplacePhotoCommand(Guid OrderId, byte[] PhotoBytes, string ContentType) : IRequest<Order?>;

public class AdminReplacePhotoCommandHandler(
    IAppDbContext db,
    IFileStorage fileStorage,
    IPhotoThumbnailGenerator thumbnailGenerator) : IRequestHandler<AdminReplacePhotoCommand, Order?>
{
    public async Task<Order?> Handle(AdminReplacePhotoCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOrderForAdminAsync(db, request.OrderId, ct);
        if (order is null) return null;

        var url = await fileStorage.SaveAsync(request.PhotoBytes, request.ContentType, "photos", ct);
        var thumb = thumbnailGenerator.Generate(new StoredFile(request.PhotoBytes, request.ContentType));
        var thumbUrl = await fileStorage.SaveAsync(thumb.Content, thumb.ContentType, "photo-thumbs", ct);

        // "Replace" means the whole set, not "add another" — keeps the admin UI a simple
        // single-file control instead of needing its own multi-photo management. Removing via
        // the DbSet and adding via the DbSet (not order.Photos.Clear()/.Add()) is deliberate — a
        // pre-existing bug (present before #416 too, just never exercised by a test) had the
        // navigation-collection mutation confuse EF's change tracker into emitting an UPDATE for
        // the new photo's row instead of an INSERT, since it reused the removed entity's tracked
        // slot; a DbUpdateConcurrencyException followed because that row no longer existed.
        db.OrderPhotos.RemoveRange(order.Photos);
        db.OrderPhotos.Add(new OrderPhoto { OrderId = order.Id, Url = url, ThumbUrl = thumbUrl });
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOrderForAdminAsync(db, request.OrderId, ct);
    }
}
