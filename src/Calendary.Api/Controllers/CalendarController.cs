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
        IImageService imageService,
        IMapper mapper) : BaseUserController(userService)
    {

        [HttpPost]
        public async Task<IActionResult> CreateCalendar([FromBody] CalendarDto calendar)
        {
            var user = await CurrentUser.Value;

            if (user is null)
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

            if (user is null)
            {
                return Unauthorized();
            }
            var createdCalendar = await calendarService.GetCurrentAsync(user.Id);

            if (createdCalendar is null)
            {
                var entity = mapper.Map<Calendar>(new Calendar());
                await FillSettingsAsync(entity, user);
                createdCalendar = await calendarService.CreateAsync(user.Id, entity);
            }
            var result = mapper.Map<CalendarDto>(createdCalendar);
            return Ok(result);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateCalendar([FromBody] CalendarDto calendar)
        {
            var user = await CurrentUser.Value;

            if (user is null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = mapper.Map<Calendar>(calendar);


            var result = await calendarService.UpdateCalendarAsync(user.Id, entity);
            if (!result)
            {
                return StatusCode(500, "Failed to update calendar");
            }

            return Ok();
        }


        [HttpPost]
        [Route("add-cart/{calendarId:int}")]
        public async Task<IActionResult> AddToCart(int calendarId)
        {
            var user = await CurrentUser.Value;
            if (user is null)
            {
                return Unauthorized();
            }
            await calendarService.GeneratePdfAsync(user.Id, calendarId);
            await CreateThumnail(calendarId);
            await calendarService.MakeNotCurrentAsync(user.Id, calendarId);
            return Ok();
        }

        private async Task FillSettingsAsync(Calendar calendar, User user)
        {
            var userSetting = await userSettingService.GetSettingsByUserIdAsync(user.Id);

            calendar.Year = 2025;
            calendar.CountryId = Country.Ukraine.Id;

            if (userSetting != null)
            {
                calendar.FirstDayOfWeek = userSetting.FirstDayOfWeek;
                calendar.LanguageId = userSetting.LanguageId;
            }
        }


        [HttpGet("generate/{calendarId:int}")]
        public async Task<IActionResult> GeneratePdf(int calendarId)
        {
            var user = await CurrentUser.Value;

            if (user is null)
            {
                return Unauthorized();
            }

            await calendarService.GeneratePdfAsync(user.Id, calendarId);
           
            await CreateThumnail(calendarId);
            return Ok();
        }

        private async Task CreateThumnail(int calendarId)
        {
            var images = await imageService.GetAllByCalendarIdAsync(calendarId);

            var imagePaths = images.Select(p => p.ImageUrl).ToList().ToArray();
            var fileName = $"{Guid.NewGuid()}.jpg";
            var thumbnailPath = await imageService.CreateCombinedThumbnailAsync(imagePaths, fileName, 117, 156);
            await calendarService.UpdatePreviewPathAsync(calendarId, thumbnailPath);
        }
    }
}
