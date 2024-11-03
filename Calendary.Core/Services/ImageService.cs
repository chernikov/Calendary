using Calendary.Model;
using Calendary.Repos.Repositories;
using System.Globalization;

namespace Calendary.Core.Services;

public interface IImageService
{
    Task SaveAsync(Image image);

    Task<IEnumerable<Image>> GetAllByCalendarIdAsync(int calendarId);

    Task<Image?> GetByIdAsync(int imageId);
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

    public Task DeleteAsync(Image image)
        => imageRepository.DeleteAsync(image.Id);
}
