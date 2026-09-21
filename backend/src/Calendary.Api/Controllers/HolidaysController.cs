using Calendary.Api.Dtos;
using Calendary.Application.Holidays;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

/// Public holidays for a given year, across all countries — the style-dates step fetches this
/// once and filters client-side by the customer's currently-selected countries, so toggling a
/// checkbox updates the calendar preview instantly without a round trip (see #364/#366).
[ApiController]
[Route("api/holidays")]
[AllowAnonymous]
public class HolidaysController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HolidayDto>>> Get([FromQuery] int year, CancellationToken ct)
    {
        var holidays = await sender.Send(new ListHolidaysForYearQuery(year), ct);
        return Ok(holidays.Select(h => h.ToDto()).ToList());
    }
}
