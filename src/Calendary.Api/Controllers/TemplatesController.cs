using Calendary.Api.Dtos;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(
        ITemplateRepository templateRepository,
        ILogger<TemplatesController> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of templates with optional filtering and sorting
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes
    public async Task<ActionResult<PagedResult<TemplateDto>>> GetTemplates(
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "popularity")
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var (items, totalCount) = await _templateRepository.GetPagedAsync(
                category, search, page, pageSize, sortBy);

            var templateDtos = items.Select(MapToDto);

            var result = new PagedResult<TemplateDto>
            {
                Items = templateDtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates");
            return StatusCode(500, "An error occurred while retrieving templates");
        }
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 600)] // Cache for 10 minutes
    public async Task<ActionResult<TemplateDetailDto>> GetTemplateById(int id)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);

            if (template == null)
            {
                return NotFound($"Template with ID {id} not found");
            }

            if (!template.IsActive)
            {
                return NotFound($"Template with ID {id} is not available");
            }

            return Ok(MapToDetailDto(template));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {TemplateId}", id);
            return StatusCode(500, "An error occurred while retrieving the template");
        }
    }

    /// <summary>
    /// Get list of available categories
    /// </summary>
    [HttpGet("categories")]
    [ResponseCache(Duration = 3600)] // Cache for 1 hour
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        try
        {
            var categories = await _templateRepository.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Get featured templates
    /// </summary>
    [HttpGet("featured")]
    [ResponseCache(Duration = 1800)] // Cache for 30 minutes
    public async Task<ActionResult<IEnumerable<TemplateDto>>> GetFeatured([FromQuery] int count = 5)
    {
        try
        {
            if (count < 1 || count > 20) count = 5;

            var templates = await _templateRepository.GetFeaturedAsync(count);
            var templateDtos = templates.Select(MapToDto);

            return Ok(templateDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured templates");
            return StatusCode(500, "An error occurred while retrieving featured templates");
        }
    }

    private static TemplateDto MapToDto(Template template)
    {
        return new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            Price = template.Price,
            PreviewImageUrl = template.PreviewImageUrl
        };
    }

    private static TemplateDetailDto MapToDetailDto(Template template)
    {
        return new TemplateDetailDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            Price = template.Price,
            PreviewImageUrl = template.PreviewImageUrl,
            TemplateData = template.TemplateData,
            SortOrder = template.SortOrder,
            CreatedAt = template.CreatedAt
        };
    }
}
