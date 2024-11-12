using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/error")]
public class ErrorController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        Thread.Sleep(10000);

        return NotFound("What?");
    }
}
