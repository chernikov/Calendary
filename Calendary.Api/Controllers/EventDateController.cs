using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/[controller]")]
public class EventDatesController(IUserService userService,
                                IEventDateService eventDateService,
                                IMapper mapper) : BaseUserController(userService)
{

    [HttpGet]
    public async Task<IActionResult> GetEventDates()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var eventDates = await eventDateService.GetAllEventDatesAsync(user.Id);
        var result = mapper.Map<List<EventDateDto>>(eventDates);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventDate(int id)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var eventDate = await eventDateService.GetEventDateByIdAsync(user.Id, id);
        if (eventDate is null)
        {
            return NotFound();
        }
        var result = mapper.Map<EventDateDto>(eventDate);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEventDate(EventDateDto eventDate)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var entity = mapper.Map<EventDate>(eventDate);
        var createdEventDate = await eventDateService.CreateEventDateAsync(user.Id, entity);
        var result = mapper.Map<EventDateDto>(createdEventDate); 
        return CreatedAtAction(nameof(GetEventDate), new { id = result.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateEventDate(EventDateDto eventDate)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var entity = mapper.Map<EventDate>(eventDate);
        var updatedEventDate = await eventDateService.UpdateEventDateAsync(user.Id, entity);
        if (updatedEventDate is null)
        {
            return NotFound();
        }
        var result = mapper.Map<EventDateDto>(updatedEventDate);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEventDate(int id)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        await eventDateService.DeleteEventDateAsync(user.Id, id);
        return NoContent();
    }
}
