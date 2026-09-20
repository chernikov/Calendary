using Calendary.Api.Dtos;
using Calendary.Application.PromptLibrary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

/// The prompt library shown in the user-facing sheet-plan wizard: themes (папки промптів) with
/// their prompts, plus the image styles that can be overlaid on any prompt.
[ApiController]
[Route("api/prompt-library")]
[AllowAnonymous]
public class PromptLibraryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PromptLibraryDto>> Get(CancellationToken ct)
    {
        var library = await sender.Send(new GetPromptLibraryQuery(), ct);
        return Ok(new PromptLibraryDto(
            library.Themes.Select(t => t.ToDto()).ToList(),
            library.Styles.Select(s => s.ToDto()).ToList()));
    }
}
