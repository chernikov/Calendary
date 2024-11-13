using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;


[ApiController]
[Route("api/nova-search")]
public class NovaSearchController(INovaPostService novaPostService) : ControllerBase
{

    [HttpGet("city")]
    public async Task<IActionResult> SearchCity([FromQuery] string search)
    {
        var cityList = await novaPostService.SearchCityAsync(search);
        return Ok(cityList);
    }

    [HttpGet("warehouse")]
    public async Task<IActionResult> SearchWarehouse([FromQuery] string city, [FromQuery] string search)
    {
        var warehouseList = await novaPostService.SearchWarehouseAsync(city, search);
        return Ok(warehouseList);
    }
}
