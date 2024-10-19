using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/holiday")]
public class HolidayController : BaseUserController
{
    private readonly IHolidayService _holidayService;
    private readonly IMapper _mapper;

    public HolidayController(IHolidayService holidayService, IUserService userService, IMapper mapper) : base(userService)
    {
        _holidayService = holidayService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllHolidays()
    {
        var holidays = await _holidayService.GetAllHolidaysAsync();
        var result = _mapper.Map<List<HolidayDto>>(holidays);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddHoliday([FromBody] HolidayDto holiday)
    {
        var holidayEntity = _mapper.Map<Holiday>(holiday);
        var addedHoliday = await _holidayService.AddHolidayAsync(holidayEntity);
        var result = _mapper.Map<HolidayDto>(addedHoliday);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateHoliday([FromBody] HolidayDto holiday)
    {
        var holidayEntity = _mapper.Map<Holiday>(holiday);
        var updatedHoliday = await _holidayService.UpdateHolidayAsync(holidayEntity);
        var result = _mapper.Map<HolidayDto>(updatedHoliday);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHoliday(int id)
    {
        await _holidayService.DeleteHolidayAsync(id);
        return Ok();
    }
}
