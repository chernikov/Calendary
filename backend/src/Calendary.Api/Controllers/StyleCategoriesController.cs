using Calendary.Api.Dtos;
using Calendary.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/style-categories")]
[AllowAnonymous]
public class StyleCategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StyleCategoryDto>>> List()
    {
        var categories = await db.StyleCategories.OrderBy(c => c.SortOrder).ToListAsync();
        return Ok(categories.Select(c => c.ToDto()).ToList());
    }
}
