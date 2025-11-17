using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Calendary.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/prompts")]
public class PromptController : ControllerBase
{
    private readonly IPromptEnhancerService _promptEnhancerService;
    private readonly ILogger<PromptController> _logger;

    public PromptController(
        IPromptEnhancerService promptEnhancerService,
        ILogger<PromptController> logger)
    {
        _promptEnhancerService = promptEnhancerService;
        _logger = logger;
    }

    /// <summary>
    /// Enhance a user prompt using AI (OpenAI or Anthropic)
    /// </summary>
    /// <param name="request">Prompt to enhance</param>
    /// <returns>Enhanced prompt with suggestions</returns>
    [HttpPost("enhance")]
    [ProducesResponseType(typeof(PromptEnhanceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EnhancePrompt([FromBody] PromptEnhanceRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _promptEnhancerService.EnhancePromptAsync(request.Prompt, userId);

            var dto = new PromptEnhanceResponseDto
            {
                EnhancedPrompt = response.EnhancedPrompt,
                Suggestions = response.Suggestions
            };

            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Rate limit"))
        {
            _logger.LogWarning("Rate limit exceeded for user: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status429TooManyRequests, new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("API key"))
        {
            _logger.LogError("API configuration error: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "AI service is not properly configured" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing prompt: {Prompt}", request.Prompt);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while enhancing the prompt" });
        }
    }
}
