using Calendary.Model;
using Calendary.Repos.Repositories;
using System.Globalization;

namespace Calendary.Core.Services;

public interface IImageService
{
    Task<short> GetNotFilledMonthAsync(int calendarId);

    Task SaveAsync(Image image);

    Task<IEnumerable<Image>> GetAllByCalendarIdAsync(int calendarId);

    Task<Image?> GetByIdAsync(int imageId);

    Task DeleteImagesByCalendarIdAsync(int calendarId);

    Task DeleteAsync(Image image);
    
}

public class ImageService(IImageRepository imageRepository) : IImageService
{
    public Task SaveAsync(Image image)
        => imageRepository.AddAsync(image);


    public Task<IEnumerable<Image>> GetAllByCalendarIdAsync(int calendarId)
        => imageRepository.GetAllByCalendarIdAsync(calendarId);
    
    public Task<Image?> GetByIdAsync(int imageId)
        => imageRepository.GetByIdAsync(imageId);

    public async Task DeleteAsync(Image image)
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
}
