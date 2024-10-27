using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TempUserController : BaseUserController
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    private readonly ICalendarService _calendarService;
    private readonly IUserSettingService _userSettingService;

    public TempUserController(IUserService userService, IAuthService authService, 
        ICalendarService calendarService, 
        IUserSettingService userSettingService, IMapper mapper) : base(userService)
    {
        _userService = userService;
        _authService = authService;
        _calendarService = calendarService;
        _userSettingService = userSettingService;
        _mapper = mapper;
    }

    [HttpPost("init")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
    public async Task<IActionResult> Init()
    {
        var user = await CurrentUser.Value;
        if (user is not null)
        {
            var currentCalendar = await _calendarService.GetCurrentAsync(user.Id);
            if (currentCalendar is null)
            {
                await CreateCalendarAsync(user);
            }
            return NoContent();
        }

        string password = Guid.NewGuid().ToString(); // Генерація пароля  

        // Створення тимчасового користувача  
        var newUser = new User
        {
            Email = $"user_{Guid.NewGuid()}@gmail.com",
            IsTemporary = true
        };

        var createdUser = await _userService.RegisterUserAsync(newUser, password);
        var result = _mapper.Map<UserDto>(newUser);
        result.Token = await _authService.GenerateJwtTokenAsync(createdUser);
        await CreateCalendarAsync(createdUser);
        return Created("", result);
    }

    private async Task CreateCalendarAsync(User user)
    {
        var calendar = new CalendarDto();
        var entity = _mapper.Map<Calendar>(calendar);
        await FillSettingsAsync(entity, user);
        var createdCalendar = await _calendarService.CreateAsync(user.Id, entity);
    }

    private async Task FillSettingsAsync(Calendar calendar, User user)
    {
        var userSetting = await _userSettingService.GetSettingsByUserIdAsync(user.Id);

        calendar.Year = 2025;
        calendar.CountryId = Country.Ukraine.Id;

        if (userSetting != null)
        {
            calendar.FirstDayOfWeek = userSetting.FirstDayOfWeek;
            calendar.LanguageId = userSetting.LanguageId;
        }
    }
}
