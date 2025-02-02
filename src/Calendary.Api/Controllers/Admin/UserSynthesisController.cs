using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/user/{userId:int}/synthesis")]
public class UserSynthesisController : BaseAdminController
{
    private readonly ISynthesisService _synthesisService;
    private readonly IMapper _mapper;

    public UserSynthesisController(IUserService userService, ISynthesisService synthesisService, IMapper mapper) : base(userService)
    {
        _synthesisService = synthesisService;
        _mapper = mapper;
    }


    // GET: api/admin/user/{userId:int}/synthesis
    [HttpGet("{trainingId:int}")]
    public async Task<IActionResult> GetSynthesisesByTrainingId(int trainingId)
    {
        var synthesises = await _synthesisService.GetByTrainingIdAsync(trainingId);
        var result = _mapper.Map<List<SynthesisDto>>(synthesises);
        return Ok(synthesises);
    }


}
