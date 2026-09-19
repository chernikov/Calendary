using Calendary.Domain;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// One-time conversion of the rows written before IFileStorage existed, when photos and generated
/// sheets were stored inline as base64 data: URLs. Runs at startup (Program.cs) and is a no-op
/// once no data: URLs remain, so it can stay in place across deploys.
public static class MediaMigrator
{
    // Rows are multi-MB each — small batches keep peak memory bounded.
    private const int BatchSize = 20;

    public static async Task ConvertInlineImagesAsync(
        AppDbContext db, IFileStorage fileStorage, ILogger logger, CancellationToken ct = default)
    {
        var photos = await ConvertOrderPhotosAsync(db, fileStorage, logger, ct);

        var sheets = await ConvertAsync(
            db,
            () => db.Sheets.Where(s => s.ImageUrl != null && s.ImageUrl.StartsWith("data:")),
            s => s.ImageUrl,
            (s, url) => s.ImageUrl = url,
            "sheets",
            fileStorage,
            logger,
            ct);

        if (photos + sheets > 0)
        {
            logger.LogInformation(
                "Converted {Photos} inline photo(s) and {Sheets} inline sheet image(s) to file storage.",
                photos, sheets);
        }
    }

    // OrderPhoto.Url/ThumbUrl are non-nullable (unlike the old nullable Order.PhotoUrl), so an
    // unparseable legacy value removes the row rather than nulling a field — a row's only reason
    // to exist is holding a URL. This also generates a real ThumbUrl from the decoded bytes,
    // replacing the same-as-Url placeholder the AddOrderPhotos migration backfilled it with.
    private static async Task<int> ConvertOrderPhotosAsync(
        AppDbContext db, IFileStorage fileStorage, ILogger logger, CancellationToken ct)
    {
        var converted = 0;

        while (true)
        {
            var batch = await db.OrderPhotos.Where(p => p.Url.StartsWith("data:")).Take(BatchSize).ToListAsync(ct);
            if (batch.Count == 0)
            {
                return converted;
            }

            foreach (var photo in batch)
            {
                if (DataUrl.TryParse(photo.Url, out var contentType, out var bytes))
                {
                    var original = new StoredFile(bytes, contentType);
                    photo.Url = await fileStorage.SaveAsync(original.Content, original.ContentType, "photos", ct);
                    var thumb = PhotoThumbnailGenerator.Generate(original);
                    photo.ThumbUrl = await fileStorage.SaveAsync(thumb.Content, thumb.ContentType, "photo-thumbs", ct);
                    converted++;
                }
                else
                {
                    logger.LogWarning("Discarding inline photo image (not a base64 data URL).");
                    db.OrderPhotos.Remove(photo);
                }
            }

            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
        }
    }

    private static async Task<int> ConvertAsync<TEntity>(
        AppDbContext db,
        Func<IQueryable<TEntity>> query,
        Func<TEntity, string?> getUrl,
        Action<TEntity, string?> setUrl,
        string category,
        IFileStorage fileStorage,
        ILogger logger,
        CancellationToken ct)
        where TEntity : class
    {
        var converted = 0;

        while (true)
        {
            var batch = await query().Take(BatchSize).ToListAsync(ct);
            if (batch.Count == 0)
            {
                return converted;
            }

            foreach (var entity in batch)
            {
                try
                {
                    if (!DataUrl.TryParse(getUrl(entity), out var contentType, out var bytes))
                    {
                        throw new NotSupportedException("not a base64 data URL");
                    }

                    setUrl(entity, await fileStorage.SaveAsync(bytes, contentType, category, ct));
                    converted++;
                }
                catch (NotSupportedException ex)
                {
                    // Unreadable payload: drop it rather than leave a row that can never render.
                    logger.LogWarning("Discarding inline {Category} image ({Reason}).", category, ex.Message);
                    setUrl(entity, null);
                }
            }

            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
        }
    }
}
