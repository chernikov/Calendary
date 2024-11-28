using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;


[ApiController]
[Route("api/admin/fluxmodels")]
public class FluxModelController : BaseAdminController
{
    private readonly IFluxModelService _fluxModelService;
    private readonly IMapper _mapper;

    public FluxModelController(IUserService userService, IFluxModelService fluxModelService, IMapper mapper) : base(userService)
    {
        _fluxModelService = fluxModelService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var models = await _fluxModelService.GetAllAsync(page, pageSize);
        var results = _mapper.Map<List<AdminFluxModelDto>>(models);
        return Ok(models);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var models = await _fluxModelService.GetFullAsync(id);
        var results = _mapper.Map<AdminFluxModelDto>(models);
        return Ok(models);
    }
}

