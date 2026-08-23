using Calendary.Api.Dtos;
using Calendary.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/nova-poshta")]
[AllowAnonymous]
public class NovaPoshtaController(INovaPoshtaService novaPoshta) : ControllerBase
{
    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<string>>> Cities([FromQuery] string query = "")
    {
        return Ok(await novaPoshta.SearchCitiesAsync(query));
    }

    [HttpGet("warehouses")]
    public async Task<ActionResult<IReadOnlyList<NovaPoshtaWarehouseDto>>> Warehouses([FromQuery] string city)
    {
        var warehouses = await novaPoshta.GetWarehousesAsync(city);
        return Ok(warehouses.Select(w => new NovaPoshtaWarehouseDto(w.Number, w.Address, w.ClosesAt)).ToList());
    }
}
