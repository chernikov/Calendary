using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/trainings")]
public class TrainingController : ControllerBase
{
    private readonly ITrainingService _trainingService;

    public TrainingController(ITrainingService trainingService)
    {
        _trainingService = trainingService;
    }

    // DELETE: api/admin/trainings/{trainingId}
    // Soft‑видалення: встановлюємо IsDeleted = true
    [HttpDelete("{trainingId:int}")]
    public async Task<IActionResult> DeleteTraining(int trainingId)
    {
        await _trainingService.SoftDeleteAsync(trainingId);
        return NoContent();
    }
}
