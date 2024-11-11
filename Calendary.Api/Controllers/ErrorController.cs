using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/error")]
public class ErrorController : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        Thread.Sleep(10000);

        return NotFound("What?");
    }
}
