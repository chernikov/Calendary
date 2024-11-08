﻿using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/error")]
public class ErrorController : Controller
{
    [HttpGet]
    public Task<IActionResult> Get()
    {
        throw new Exception();
    }
}
