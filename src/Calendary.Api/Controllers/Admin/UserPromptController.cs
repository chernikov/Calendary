using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;


[ApiController]
[Route("api/admin/user/{userId:int}/prompt")]
public class UserPromptController : BaseAdminController
{
    private readonly ISynthesisService _synthesisService;
    private readonly IMapper _mapper;

    
    public UserPromptController(IUserService userService,
        ISynthesisService synthesisService,
        IMapper mapper) : base(userService)
    {
        _synthesisService = synthesisService;
        _mapper = mapper;
    }


    // GET: api/admin/user/{userId:int}/synthesis
    [HttpGet("{trainingId:int}")]
    public async Task<IActionResult> GetSynthesisesByTrainingId(int trainingId)
    {
        var synthesises = await _synthesisService.GetByTrainingIdAsync(trainingId);

        // Group synthesises by their associated prompt.
        var groupedPrompts = synthesises
            .GroupBy(s => s.Prompt) // Group by the Prompt property. Note: this may be null.
            .Select(group =>
            {
                // Map the associated prompt if available, otherwise create a dummy prompt DTO.
                var promptDto = group.Key != null
                    ? _mapper.Map<PromptDto>(group.Key)
                    : new PromptDto
                    {
                        Id = 0,
                        ThemeId = 0,
                        Text = "No prompt",
                        CategoryId = 0
                    };

                // Map the synthesises in this group to their DTOs.
                promptDto.Synthesises = _mapper.Map<List<SynthesisDto>>(group.ToList());

                return promptDto;
            })
            .ToList();

        return Ok(groupedPrompts);
    }
}