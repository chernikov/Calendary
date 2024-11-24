using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;


[Authorize(Roles = "User")]
[ApiController]
[Route("api/prompt-theme")]
public class PromptThemeController : ControllerBase
{
    private readonly IPromptThemeService _promptThemeService;
    private readonly IMapper _mapper;

    public PromptThemeController(IPromptThemeService promptThemeService, IMapper mapper)
    {
        _promptThemeService = promptThemeService;
        _mapper = mapper;
    }

    /// <summary>
    /// Отримати всі теми (окрім default)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Отримання всіх тем (фільтруємо default)
        var themes = await _promptThemeService.GetAllAsync();
        var filteredThemes = themes.Where(t => t.Name.ToLower() != "default").ToList();

        // Маппінг до DTO
        var result = _mapper.Map<List<PromptThemeDto>>(filteredThemes);
        return Ok(result);
    }
}

