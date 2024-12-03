using Calendary.Repos.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using ImageModel = Calendary.Model.Image;
using Calendary.Core.Providers;

namespace Calendary.Core.Services;

public interface IImageService
{
    Task<short> GetNotFilledMonthAsync(int calendarId);

    Task SaveAsync(ImageModel image);

    Task<IEnumerable<ImageModel>> GetAllByCalendarIdAsync(int calendarId);

    Task<ImageModel?> GetByIdAsync(int imageId);

    Task DeleteImagesByCalendarIdAsync(int calendarId);

    Task DeleteAsync(ImageModel image);

    Task<string> CreateCombinedThumbnailAsync(string[] imagePaths, 
        string fileName, int thumbnailWidth, int thumbnailHeight);


}

public class ImageService(IImageRepository imageRepository, IPathProvider pathProvider) : IImageService
{

    private static string ThumbnailDirPath = "uploads";
    public Task SaveAsync(ImageModel image)
        => imageRepository.AddAsync(image);


    public Task<IEnumerable<ImageModel>> GetAllByCalendarIdAsync(int calendarId)
        => imageRepository.GetAllByCalendarIdAsync(calendarId);
    
    public Task<ImageModel?> GetByIdAsync(int imageId)
        => imageRepository.GetByIdAsync(imageId);

    public async Task DeleteAsync(ImageModel image)
    {
        var file = image.ImageUrl;
        if (File.Exists(file))
        {
            File.Delete(file);
        }
        await imageRepository.DeleteAsync(image.Id);
    }

    public async Task DeleteImagesByCalendarIdAsync(int calendarId)
    {
        var images = await imageRepository.GetAllByCalendarIdAsync(calendarId);
        foreach(var image in images)
        {
            await DeleteAsync(image);
        }
    }

    public async Task<short> GetNotFilledMonthAsync(int calendarId)
    {
        var images = await imageRepository.GetAllByCalendarIdAsync(calendarId);

        if (images.Count() > 0)
        {
            // Отримуємо всі місяці, які вже заповнені
            var filledMonths = images.Select(img => img.MonthNumber).ToList();

            // Шукаємо найменший місяць від 0 до 11, який відсутній
            for (short month = 0; month < 12; month++)
            {
                if (!filledMonths.Contains(month))
                {
                    return month;
                }
            }
            return -1;
        }
        return 0;
        
    }

    public async Task<string> CreateCombinedThumbnailAsync(string[] imagePaths, string fileName,
        int thumbnailWidth, int thumbnailHeight)
    {
        int imagesPerRow = 6; // Наприклад, 6 зображень в рядку
        int rows = (int)Math.Ceiling(imagePaths.Length / (double)imagesPerRow);

        // Розраховуємо розмір фінального зображення
        int combinedWidth = imagesPerRow * thumbnailWidth;
        int combinedHeight = rows * thumbnailHeight;

        // Створюємо фінальне зображення
        using (var combinedImage = new Image<Rgba32>(combinedWidth, combinedHeight))
        {
            int x = 0, y = 0;

            foreach (var path in imagePaths)
            {
                var destPath = pathProvider.MapPath(path);
                // Завантажуємо кожне зображення
                using (var image = Image.Load<Rgba32>(destPath))
                {
                    // Масштабуємо до розміру прев'ю
                    image.Mutate(i => i.Resize(thumbnailWidth, thumbnailHeight));

                    // Малюємо зображення на фінальному полотні
                    combinedImage.Mutate(ctx => ctx.DrawImage(image, new Point(x, y), 1f));
                }

                // Зсуваємо позицію
                x += thumbnailWidth;
                if (x >= combinedWidth)
                {
                    x = 0;
                    y += thumbnailHeight;
                }
            }

            // Зберігаємо фінальне зображення
            string outputFileName = Path.Combine(ThumbnailDirPath, fileName);
            var ouputPath = pathProvider.MapPath(outputFileName);

            // Create directory if not exists
            var directory = pathProvider.MapPath(ThumbnailDirPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await combinedImage.SaveAsync(ouputPath);
            return outputFileName;
        }
    }
}
