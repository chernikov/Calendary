using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/user/{userId:int}/flux-models")]
public class UserFluxModelController : ControllerBase
{
    private readonly IFluxModelService _fluxModelService;
    private readonly IPhotoService _photoService;
    private readonly IMapper _mapper;

    public UserFluxModelController(IFluxModelService fluxModelService, 
        IPhotoService photoService,  
        IMapper mapper)
    {
        _fluxModelService = fluxModelService;
        _photoService = photoService;
        _mapper = mapper;
    }

    // GET: api/admin/user/{userId}/flux-models
    [HttpGet]
    public async Task<IActionResult> GetUserFluxModels(int userId)
    {
        var fluxModels = await _fluxModelService.GetListByUserIdAsync(userId);
        var result = _mapper.Map<List<FluxModelDto>>(fluxModels);
        return Ok(result);
    }


    // DELETE: api/admin/user/{userId}/flux-models/{fluxModelId}
    // Soft‑видалення: встановлення прапорця isDeleted = true
    [HttpDelete("{fluxModelId:int}")]
    public async Task<IActionResult> DeleteFluxModel(int fluxModelId)
    {
        await _fluxModelService.SoftDeleteAsync(fluxModelId);
        return NoContent();
    }

    // POST: api/admin/user/{userId}/flux-models
    [HttpPost]
    public async Task<IActionResult> Create(int userId, [FromBody] CreateFluxModelDto model)
    {
        var fluxModel = _mapper.Map<FluxModel>(model);
        fluxModel.UserId = userId;    
        await _fluxModelService.CreateAsync(fluxModel);
        var entity = await _fluxModelService.GetByIdAsync(fluxModel.Id);
        var result = _mapper.Map<FluxModelDto>(entity);
        return Ok(result);
    }

    // GET: api/admin/user/{userId}/flux-models/{fluxModelId}/photos
    [HttpGet("{fluxModelId:int}/photos")]
    public async Task<IActionResult> GetPhotos(int userId, int fluxModelId)
    {
        // Метод GetPhotosAsync має повернути список фотографій для даної flux моделі,
        // враховуючи, що потрібна додаткова фільтрація за userId, якщо необхідно.
        var photos = await _photoService.GetByFluxModelIdAsync(fluxModelId);
        var result = _mapper.Map<List<PhotoDto>>(photos);
        return Ok(result);
    }

    // PUT: api/admin/user/{userId}/flux-models/change-name
    [HttpPut("change-name")]
    public async Task<IActionResult> ChangeName(int userId, [FromBody] FluxModelDto model)
    {
        // Можна додатково перевірити, чи належить ця flux модель користувачу userId
        var entry = _mapper.Map<FluxModel>(model);
        await _fluxModelService.ChangeNameAsync(entry);
        return NoContent();
    }

    
}
