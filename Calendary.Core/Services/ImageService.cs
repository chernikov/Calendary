using Calendary.Model;
using Calendary.Repos.Repositories;
using System.Globalization;

namespace Calendary.Core.Services;

public interface IImageService
{
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
}
