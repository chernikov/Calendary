using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/prompt")]
public class PromptController : ControllerBase
{
    private readonly IPromptService _promptService;
    private readonly IMapper _mapper;

    public PromptController(IPromptService promptService, IMapper mapper)
    {
        _promptService = promptService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? themeId = null, int? ageGender = null)
    {
        IEnumerable<Prompt> prompts;

        prompts = await _promptService.GetFullAllAsync(themeId, ageGender);
        var result = _mapper.Map<List<PromptDto>>(prompts);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var prompt = await _promptService.GetByIdAsync(id);
        if (prompt == null)
        {
            return NotFound();
        }
        var result = _mapper.Map<PromptDto>(prompt);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PromptDto promptDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var prompt = _mapper.Map<Prompt>(promptDto);
        await _promptService.CreateAsync(prompt);
        var createdPrompt = await _promptService.GetByIdAsync(prompt.Id);
        var result = _mapper.Map<PromptDto>(createdPrompt);
        return CreatedAtAction(nameof(Get), new { id = prompt.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] PromptDto promptDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (promptDto.Id.HasValue)
        {
            var entity = await _promptService.GetByIdAsync(promptDto.Id.Value);
            if (entity == null)
            {
                return NotFound();
            }
        } else
        {
            return NotFound();
        }

        var prompt = _mapper.Map<Prompt>(promptDto);
        await _promptService.UpdateAsync(prompt);
        var updatedPrompt = await _promptService.GetByIdAsync(prompt.Id);
        var result = _mapper.Map<PromptDto>(updatedPrompt);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var prompt = await _promptService.GetByIdAsync(id);
        if (prompt == null)
        {
            return NotFound();
        }

        await _promptService.DeleteAsync(id);
        return NoContent();
    }
}