using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguageController(ILanguageService languageService, IMapper mapper) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetAllLanguages()
    {
        var languages = await languageService.GetAllLanguagesAsync();
        var result = mapper.Map<IEnumerable<LanguageDto>>(languages);
        return Ok(result);
    }
}
