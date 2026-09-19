using Calendary.Api.Dtos;
using Calendary.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Api.Controllers;

/// The prompt library shown in the user-facing sheet-plan wizard: themes (папки промптів) with
/// their prompts, plus the image styles that can be overlaid on any prompt.
[ApiController]
[Route("api/prompt-library")]
[AllowAnonymous]
public class PromptLibraryController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PromptLibraryDto>> Get()
    {
        var themes = await db.PromptThemes
            .Include(t => t.Prompts)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
        var styles = await db.ImageStyles.OrderBy(s => s.SortOrder).ToListAsync();

        return Ok(new PromptLibraryDto(
            themes.Select(t => t.ToDto()).ToList(),
            styles.Select(s => s.ToDto()).ToList()));
    }
}
