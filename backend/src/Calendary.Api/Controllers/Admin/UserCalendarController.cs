using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Calendary.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/user/{userId:int}/calendars")]
public class UserCalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;
    private readonly IMapper _mapper;

    public UserCalendarController(ICalendarService calendarService, IMapper mapper)
    {
        _calendarService = calendarService;
        _mapper = mapper;
    }

    // GET: api/admin/user/{userId}/calendars
    [HttpGet]
    public async Task<IActionResult> GetUserCalendars(int userId)
    {
        // Отримання календарів для користувача
        
        var calendars = await _calendarService.GetByUserIdAsync(userId);
        var result = _mapper.Map<List<CalendarDto>>(calendars);
        return Ok(calendars);
    }

    // Тут можна додати методи для створення/редагування/видалення календаря, коли буде потрібно
}