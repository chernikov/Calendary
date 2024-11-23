using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Model.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Authorize(Roles ="User")]
[Route("api/flux-model")]
public class FluxModelController : BaseUserController
{
    private readonly IFluxModelService _fluxModelService;
    private readonly IMapper _mapper;

    public FluxModelController(IUserService userService, IFluxModelService fluxModelService, IMapper mapper) : base(userService)
    {
        _fluxModelService = fluxModelService;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<IActionResult> GetCurrent()
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }
        var fluxModel = await _fluxModelService.GetCurrentByUserIdAsync(user.Id);
        if (fluxModel == null)
        {
            return NotFound();
        }
        var result = _mapper.Map<FluxModelDto>(fluxModel);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var fluxModel = await _fluxModelService.GetByIdAsync(id);
        if (fluxModel == null)
        {
            return NotFound();
        }
        var result = _mapper.Map<FluxModelDto>(fluxModel);
        return Ok(result);
    }

    // Створення нового FluxModel
    [HttpPost]
    public async Task<IActionResult> Create(CreateFluxModelDto model)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        // Генерація назви моделі
        string randomName = GenerateRandomName();

        var fluxModel = new FluxModel
        {
            UserId = user.Id,
            Gender = model.Gender == "male" ? GenderEnum.Male : GenderEnum.Female,
            Status = "creating",
            Name = randomName,
            IsPaid = true
        };
        await _fluxModelService.CreateAsync(fluxModel);

        var entity = await _fluxModelService.GetByIdAsync(fluxModel.Id);

        var result = _mapper.Map<FluxModelDto>(entity);
        return CreatedAtAction(nameof(GetById), new { id = fluxModel.Id }, result);
    }

  

    // Оновлення статусу FluxModel
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var fluxModel = await _fluxModelService.GetByIdAsync(id);
        if (fluxModel == null)
        {
            return NotFound();
        }
        fluxModel.Status = status;
        await _fluxModelService.UpdateStatusAsync(fluxModel);
        var result = _mapper.Map<FluxModelDto>(fluxModel);
        return Ok(result);
    }



    private string GenerateRandomName()
    {
        // Списки кольорів та істот
        string[] colors = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Black", "White", "Gray" };
        string[] creatures = { "Tiger", "Eagle", "Dolphin", "Dragon", "Unicorn", "Lion", "Wolf", "Bear", "Hawk", "Fox" };

        var random = new Random();
        string color = colors[random.Next(colors.Length)];
        string creature = creatures[random.Next(creatures.Length)];

        return $"{color} {creature}";
    }
}