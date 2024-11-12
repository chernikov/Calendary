using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/holiday")]
public class HolidayController(IHolidayService holidayService, IUserService userService, IMapper mapper) 
    : BaseUserController(userService)
{
    [HttpGet]
    public async Task<IActionResult> GetAllHolidays()
    {
        var holidays = await holidayService.GetAllHolidaysAsync();
        var result = mapper.Map<List<HolidayDto>>(holidays);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddHoliday([FromBody] HolidayDto holiday)
    {
        var holidayEntity = mapper.Map<Holiday>(holiday);
        var addedHoliday = await holidayService.AddHolidayAsync(holidayEntity);
        var result = mapper.Map<HolidayDto>(addedHoliday);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateHoliday([FromBody] HolidayDto holiday)
    {
        var holidayEntity = mapper.Map<Holiday>(holiday);
        var updatedHoliday = await holidayService.UpdateHolidayAsync(holidayEntity);
        var result = mapper.Map<HolidayDto>(updatedHoliday);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHoliday(int id)
    {
        await holidayService.DeleteHolidayAsync(id);
        return Ok();
    }
}
