using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/prompt-theme")]
public class PromptThemeController : ControllerBase
{
    private readonly IPromptThemeService _promptThemeService;
    private readonly IMapper _mapper;

    public PromptThemeController(IPromptThemeService promptThemeService, IMapper mapper)
    {
        _promptThemeService = promptThemeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var themes = await _promptThemeService.GetAllAsync();
        var result = _mapper.Map<List<PromptThemeDto>>(themes);
        return Ok(result);
    }

    [HttpGet("/{id:int}")]
    public async Task<IActionResult> Get(int id)
    {

        var theme = await _promptThemeService.GetByIdAsync(id);
        var themeDto = _mapper.Map<PromptThemeDto>(theme);
        return Ok(themeDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PromptThemeDto themeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var theme = _mapper.Map<PromptTheme>(themeDto);
        await _promptThemeService.CreateAsync(theme);
        var createdTheme = await _promptThemeService.GetByIdAsync(theme.Id);
        var result = _mapper.Map<PromptThemeDto>(createdTheme);
        return CreatedAtAction(nameof(Get), new { id = theme.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] PromptThemeDto themeDto)
    {
                      
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var entity = await _promptThemeService.GetByIdAsync(themeDto.Id);
        if (entity is null)
        {
            return NotFound();
        }

        var theme = _mapper.Map<PromptTheme>(themeDto);
        await _promptThemeService.UpdateAsync(theme);
        var updatedTheme = await _promptThemeService.GetByIdAsync(theme.Id);
        var result = _mapper.Map<PromptThemeDto>(updatedTheme);
        return Ok(result);
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _promptThemeService.DeleteAsync(id);
        return NoContent();
    }
}
