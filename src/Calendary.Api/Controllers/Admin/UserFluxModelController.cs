using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/user/{userId:int}/flux-models")]
    public class UserFluxModelController : ControllerBase
    {
        private readonly IFluxModelService _fluxModelService;
        private readonly IMapper _mapper;

        public UserFluxModelController(IFluxModelService fluxModelService, IMapper mapper)
        {
            _fluxModelService = fluxModelService;
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
    }
}
