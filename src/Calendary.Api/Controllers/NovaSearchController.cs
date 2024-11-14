using Calendary.Core.Models;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;


[ApiController]
[Route("api/nova-search")]
public class NovaSearchController(INovaPostService novaPostService) : ControllerBase
{

    [HttpGet("city")]
    public async Task<IActionResult> SearchCity([FromQuery] string? search)
    {
        if (search is null)
        {
            return Ok(Array.Empty<NovaPostApiResponseItem>());
        }
        var cityList = await novaPostService.SearchCityAsync(search);
        return Ok(cityList);
    }

    [HttpGet("warehouse")]
    public async Task<IActionResult> SearchWarehouse([FromQuery] string? city, [FromQuery] string? search)
    {
        if (city is null)
        {
            return Ok(Array.Empty<NovaPostApiResponseItem>());
        }
        if (search is null)
        {
            search = string.Empty;
        }
        var warehouseList = await novaPostService.SearchWarehouseAsync(city, search);
        return Ok(warehouseList);
    }
}
