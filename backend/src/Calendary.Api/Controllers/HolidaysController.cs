using Calendary.Api.Dtos;
using Calendary.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Api.Controllers;

/// Public holidays for a given year, across all countries — the style-dates step fetches this
/// once and filters client-side by the customer's currently-selected countries, so toggling a
/// checkbox updates the calendar preview instantly without a round trip (see #364/#366).
[ApiController]
[Route("api/holidays")]
[AllowAnonymous]
public class HolidaysController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HolidayDto>>> Get([FromQuery] int year)
    {
        var holidays = await db.Holidays
            .Where(h => h.Year == year)
            .OrderBy(h => h.Month).ThenBy(h => h.Day)
            .ToListAsync();
        return Ok(holidays.Select(h => h.ToDto()).ToList());
    }
}
