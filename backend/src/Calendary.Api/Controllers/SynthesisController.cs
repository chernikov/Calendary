using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SynthesisController : ControllerBase
{
    private readonly IReplicateService _replicateService;

    public SynthesisController(IReplicateService replicateService)
    {
        _replicateService = replicateService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateImage([FromBody] Dtos.GenerateImageRequest request)
    {
        try
        {
            var input = GenerateImageInput.GetImageRequest(request.Prompt, request.Seed);
            var predictionId = await _replicateService.StartImageGenerationAsync(request.ModelVersion, input);
            var result = await _replicateService.GenerateImageAsync(predictionId);

            var response = new Dtos.GenerateImageResponse
            {
                ImageUrl = result.Output.FirstOrDefault(),
                Seed = result.ExtractSeedFromLogs() ?? -1,
                Status = result.Status
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
