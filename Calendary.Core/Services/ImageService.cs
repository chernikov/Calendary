﻿using Calendary.Model;
using Calendary.Repos.Repositories;
using System.Globalization;

namespace Calendary.Core.Services;

public interface IImageService
{
    Task SaveAsync(Image image);

    Task<IEnumerable<Image>> GetAllByCalendarIdAsync(int calendarId);

}

public class ImageService(IImageRepository imageRepository) : IImageService
{
    public async Task SaveAsync(Image image)
    {
        var images = await imageRepository.GetAllByCalendarIdAsync(image.CalendarId);

        int maxOrder = images.Max(i => i.Order);

        image.Order = (short)(maxOrder + 1);
        await imageRepository.AddAsync(image);
    }

    public async Task<IEnumerable<Image>> GetAllByCalendarIdAsync(int calendarId)
    {
        return (await imageRepository.GetAllByCalendarIdAsync(calendarId)).OrderBy(p => p.Order);
    }
}
