using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Route("api/holiday-presets")]
[ApiController]
public class HolidayPresetController : ControllerBase
{
    private readonly IHolidayPresetService _holidayPresetService;

    public HolidayPresetController(IHolidayPresetService holidayPresetService)
    {
        _holidayPresetService = holidayPresetService;
    }

    /// <summary>
    /// Отримати список всіх доступних пресетів свят
    /// GET /api/holiday-presets
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPresets([FromQuery] int languageId = 1)
    {
        var presets = await _holidayPresetService.GetAllPresetsAsync();

        var result = presets.Select(p => new HolidayPresetDto
        {
            Id = p.Id,
            Code = p.Code,
            Type = p.Type,
            Name = p.Translations.FirstOrDefault(t => t.LanguageId == languageId)?.Name ?? p.Code,
            Description = p.Translations.FirstOrDefault(t => t.LanguageId == languageId)?.Description
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Отримати пресет свят за кодом з деталями (включаючи свята)
    /// GET /api/holiday-presets/{code}
    /// </summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetPresetByCode(string code, [FromQuery] int languageId = 1)
    {
        var preset = await _holidayPresetService.GetPresetWithHolidaysAsync(code, languageId);

        if (preset == null)
        {
            return NotFound(new { message = $"Preset with code '{code}' not found" });
        }

        var result = new HolidayPresetDetailDto
        {
            Id = preset.Id,
            Code = preset.Code,
            Type = preset.Type,
            Name = preset.Translations.FirstOrDefault(t => t.LanguageId == languageId)?.Name ?? preset.Code,
            Description = preset.Translations.FirstOrDefault(t => t.LanguageId == languageId)?.Description,
            Holidays = preset.Holidays.Select(h => new HolidayWithTranslationDto
            {
                Id = h.Id,
                Month = h.Month,
                Day = h.Day,
                Name = h.Translations.FirstOrDefault(t => t.LanguageId == languageId)?.Name ?? h.Name ?? "Unknown",
                IsMovable = h.IsMovable,
                CalculationType = h.CalculationType,
                IsWorkingDay = h.IsWorkingDay,
                Type = h.Type
            }).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Отримати пресети свят за типом
    /// GET /api/holiday-presets/by-type/{type}
    /// </summary>
    [HttpGet("by-type/{type}")]
    public async Task<IActionResult> GetPresetsByType(string type, [FromQuery] int languageId = 1)
    {
        var presets = await _holidayPresetService.GetPresetsByTypeAsync(type);

        var result = presets.Select(p => new HolidayPresetDto
        {
            Id = p.Id,
            Code = p.Code,
            Type = p.Type,
            Name = p.Translations.FirstOrDefault(t => t.LanguageId == languageId)?.Name ?? p.Code,
            Description = p.Translations.FirstOrDefault(t => t.LanguageId == languageId)?.Description
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Застосувати пресет свят до календаря
    /// POST /api/holiday-presets/apply
    /// </summary>
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyPresetToCalendar([FromBody] ApplyPresetRequest request)
    {
        var success = await _holidayPresetService.ApplyPresetToCalendarAsync(request.CalendarId, request.PresetCode);

        if (!success)
        {
            return BadRequest(new { message = "Failed to apply preset to calendar" });
        }

        return Ok(new { message = "Preset applied successfully" });
    }
}
