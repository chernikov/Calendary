using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace Calendary.Core.Services;

public interface IEventDateService
{
    Task<IEnumerable<EventDate>> GetAllEventDatesAsync(int userId);
    Task<EventDate?> CreateEventDateAsync(int userId, EventDate eventDate);
    Task<EventDate?> GetEventDateByIdAsync(int userId, int id);
    Task<EventDate?> UpdateEventDateAsync(int userId, EventDate eventDate);

    Task DeleteEventDatesByCalendarIdAsync(int userId, int calendarId);

    Task DeleteEventDateAsync(int userId, int id);
}

public class EventDateService(IEventDateRepository eventDateRepository,
    IUserSettingRepository userSettingRepository) : IEventDateService
{

    public Task<IEnumerable<EventDate>> GetAllEventDatesAsync(int userId)
        => eventDateRepository.GetAllByUserIdAsync(userId);

    public async Task<EventDate?> CreateEventDateAsync(int userId, EventDate eventDate)
    {
        var setting = await userSettingRepository.GetByUserIdAsync(userId);
        if (setting is null)
        { 
            return null; 
        }
        eventDate.UserSettingId = setting.Id;
        await eventDateRepository.AddAsync(eventDate);
        return eventDate;
    }

    public async Task DeleteEventDateAsync(int userId, int id)
    {
        var setting = await userSettingRepository.GetByUserIdAsync(userId);
        if (setting is null)
        {
            return;
        }
        var existingEventDate = await eventDateRepository.GetByIdAsync(id);
        if (existingEventDate is null)
        {
            return;
        }
        if (setting.Id == existingEventDate.UserSettingId)
        {
            await eventDateRepository.DeleteAsync(id);
        }
    }
  

    public async Task<EventDate?> GetEventDateByIdAsync(int userId, int id)
    {
        var setting = await userSettingRepository.GetByUserIdAsync(userId);
        if (setting is null)
        {
            return null;
        }

       

        var eventDate = await eventDateRepository.GetByIdAsync(id);
        if (eventDate.UserSettingId != setting.Id)
        {
            return null;
        }
        return eventDate;

    }

    public async Task<EventDate?> UpdateEventDateAsync(int userId, EventDate eventDate)
    {
        var setting = await userSettingRepository.GetByUserIdAsync(userId);
        if (setting is null)
        {
            return null;
        }

        var existingEventDate = await eventDateRepository.GetByIdAsync(eventDate.Id);
        if (existingEventDate is null)
        {
            return null;
        }

        if (setting.Id != existingEventDate.UserSettingId)
        {
            return null;
        }

        existingEventDate.Date = eventDate.Date;
        existingEventDate.Description = eventDate.Description;
        await eventDateRepository.UpdateAsync(existingEventDate);
        return existingEventDate;
    }

    public async Task DeleteEventDatesByCalendarIdAsync(int userId, int calendarId)
    {
        var images = await eventDateRepository.GetAllByCalendarIdAsync(calendarId);
        foreach (var image in images)
        {
            await DeleteEventDateAsync(userId, image.Id);
        }
    }
}
