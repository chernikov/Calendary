using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValueController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var values = new string[] { "value1", "value2" };
        return Ok(values);
    }
}
