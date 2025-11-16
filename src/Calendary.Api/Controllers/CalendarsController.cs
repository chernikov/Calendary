using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarsController : BaseUserController
{
    private readonly IUserCalendarRepository _calendarRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<CalendarsController> _logger;

    public CalendarsController(
        IUserCalendarRepository calendarRepository,
        ITemplateRepository templateRepository,
        IUserService userService,
        ILogger<CalendarsController> logger) : base(userService)
    {
        _calendarRepository = calendarRepository;
        _templateRepository = templateRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all calendars for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserCalendarDto>>> GetCalendars()
    {
        try
        {
            var currentUser = await CurrentUser;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var calendars = await _calendarRepository.GetByUserIdAsync(currentUser.Id);
            var calendarDtos = calendars.Select(MapToDto);

            return Ok(calendarDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calendars for user");
            return StatusCode(500, "An error occurred while retrieving calendars");
        }
    }

    /// <summary>
    /// Get a specific calendar by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserCalendarDetailDto>> GetCalendar(int id)
    {
        try
        {
            var currentUser = await CurrentUser;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var calendar = await _calendarRepository.GetByIdAndUserIdAsync(id, currentUser.Id);

            if (calendar == null)
            {
                return NotFound($"Calendar with ID {id} not found");
            }

            return Ok(MapToDetailDto(calendar));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calendar {CalendarId}", id);
            return StatusCode(500, "An error occurred while retrieving the calendar");
        }
    }

    /// <summary>
    /// Create a new calendar
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserCalendarDetailDto>> CreateCalendar(
        [FromBody] CreateCalendarRequest request)
    {
        try
        {
            var currentUser = await CurrentUser;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Validate title
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title is required");
            }

            // Validate template if provided
            if (request.TemplateId.HasValue)
            {
                var template = await _templateRepository.GetByIdAsync(request.TemplateId.Value);
                if (template == null || !template.IsActive)
                {
                    return BadRequest($"Template with ID {request.TemplateId} not found or inactive");
                }
            }

            var calendar = new UserCalendar
            {
                UserId = currentUser.Id,
                Title = request.Title,
                TemplateId = request.TemplateId,
                DesignData = request.DesignData ?? "{}",
                Status = CalendarStatus.Draft
            };

            await _calendarRepository.AddAsync(calendar);

            // Reload to get template if applicable
            var createdCalendar = await _calendarRepository.GetByIdAsync(calendar.Id);

            return CreatedAtAction(
                nameof(GetCalendar),
                new { id = calendar.Id },
                MapToDetailDto(createdCalendar!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating calendar");
            return StatusCode(500, "An error occurred while creating the calendar");
        }
    }

    /// <summary>
    /// Update an existing calendar
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserCalendarDetailDto>> UpdateCalendar(
        int id,
        [FromBody] UpdateCalendarRequest request)
    {
        try
        {
            var currentUser = await CurrentUser;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var calendar = await _calendarRepository.GetByIdAndUserIdAsync(id, currentUser.Id);

            if (calendar == null)
            {
                return NotFound($"Calendar with ID {id} not found");
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                calendar.Title = request.Title;
            }

            if (request.DesignData != null)
            {
                calendar.DesignData = request.DesignData;
            }

            if (request.PreviewImageUrl != null)
            {
                calendar.PreviewImageUrl = request.PreviewImageUrl;
            }

            if (request.Status.HasValue)
            {
                calendar.Status = request.Status.Value;
            }

            await _calendarRepository.UpdateAsync(calendar);

            return Ok(MapToDetailDto(calendar));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating calendar {CalendarId}", id);
            return StatusCode(500, "An error occurred while updating the calendar");
        }
    }

    /// <summary>
    /// Delete a calendar (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCalendar(int id)
    {
        try
        {
            var currentUser = await CurrentUser;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var calendar = await _calendarRepository.GetByIdAndUserIdAsync(id, currentUser.Id);

            if (calendar == null)
            {
                return NotFound($"Calendar with ID {id} not found");
            }

            await _calendarRepository.SoftDeleteAsync(id, currentUser.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting calendar {CalendarId}", id);
            return StatusCode(500, "An error occurred while deleting the calendar");
        }
    }

    /// <summary>
    /// Duplicate a calendar
    /// </summary>
    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<UserCalendarDetailDto>> DuplicateCalendar(int id)
    {
        try
        {
            var currentUser = await CurrentUser;
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var originalCalendar = await _calendarRepository.GetByIdAndUserIdAsync(id, currentUser.Id);

            if (originalCalendar == null)
            {
                return NotFound($"Calendar with ID {id} not found");
            }

            var duplicateCalendar = new UserCalendar
            {
                UserId = currentUser.Id,
                Title = $"{originalCalendar.Title} (Copy)",
                TemplateId = originalCalendar.TemplateId,
                DesignData = originalCalendar.DesignData,
                Status = CalendarStatus.Draft
            };

            await _calendarRepository.AddAsync(duplicateCalendar);

            // Reload to get template if applicable
            var createdCalendar = await _calendarRepository.GetByIdAsync(duplicateCalendar.Id);

            return CreatedAtAction(
                nameof(GetCalendar),
                new { id = duplicateCalendar.Id },
                MapToDetailDto(createdCalendar!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating calendar {CalendarId}", id);
            return StatusCode(500, "An error occurred while duplicating the calendar");
        }
    }

    private static UserCalendarDto MapToDto(UserCalendar calendar)
    {
        return new UserCalendarDto
        {
            Id = calendar.Id,
            Title = calendar.Title,
            TemplateId = calendar.TemplateId,
            PreviewImageUrl = calendar.PreviewImageUrl,
            Status = calendar.Status,
            CreatedAt = calendar.CreatedAt,
            UpdatedAt = calendar.UpdatedAt
        };
    }

    private static UserCalendarDetailDto MapToDetailDto(UserCalendar calendar)
    {
        return new UserCalendarDetailDto
        {
            Id = calendar.Id,
            Title = calendar.Title,
            TemplateId = calendar.TemplateId,
            PreviewImageUrl = calendar.PreviewImageUrl,
            Status = calendar.Status,
            CreatedAt = calendar.CreatedAt,
            UpdatedAt = calendar.UpdatedAt,
            DesignData = calendar.DesignData
        };
    }
}
