using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController(IUserService userService,
        IUserSettingService userSettingService,
        ICalendarService calendarService,
        IMapper mapper) : BaseUserController(userService)
    {

        [HttpPost]
        public async Task<IActionResult> CreateCalendar([FromBody] CalendarDto calendar)
        {
            var user = await CurrentUser.Value;

            if (user == null)
            {
                return Unauthorized();
            }
            var entity = mapper.Map<Calendar>(calendar);
            await FillSettingsAsync(entity, user);
            var createdCalendar = await calendarService.CreateAsync(user.Id, entity);
            var result = mapper.Map<CalendarDto>(createdCalendar);
            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetCurrentCaledar()
        {
            var user = await CurrentUser.Value;

            if (user == null)
            {
                return Unauthorized();
            }
            var createdCalendar = await calendarService.GetCurrentAsync(user.Id);
            var result = mapper.Map<CalendarDto>(createdCalendar);
            return Ok(result);
        }



        private async Task FillSettingsAsync(Calendar calendar, User user)
        {
            var userSetting = await userSettingService.GetSettingsByUserIdAsync(user.Id);

            calendar.Year = 2025;

            if (userSetting != null)
            {
                calendar.FirstDayOfWeek = userSetting.FirstDayOfWeek;
                calendar.LanguageId = userSetting.LanguageId;
            }
        }
    }
}
