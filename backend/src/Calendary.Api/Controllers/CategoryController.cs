using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/category")]
public class CategoryController : BaseUserController

{
    private readonly ICategoryService categoryService;
    private readonly IMapper mapper;

    public CategoryController(IUserService userService, ICategoryService categoryService, IMapper mapper) : base(userService)
    {
        this.categoryService = categoryService;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await categoryService.GetAllAsync();
        var result = mapper.Map<IEnumerable<CategoryDto>>(categories);
        return Ok(result);
    }
}
