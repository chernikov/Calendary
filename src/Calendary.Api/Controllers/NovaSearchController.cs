using Calendary.Core.Models;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("calculate-delivery")]
    public async Task<IActionResult> CalculateDeliveryCost([FromBody] DeliveryCalculationRequest request)
    {
        try
        {
            var cost = await novaPostService.CalculateDeliveryCostAsync(request);
            return Ok(new { cost });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("create-ttn")]
    public async Task<IActionResult> CreateTTN([FromBody] CreateTTNRequest request)
    {
        try
        {
            var trackingNumber = await novaPostService.CreateInternetDocumentAsync(request);
            return Ok(new { trackingNumber });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
