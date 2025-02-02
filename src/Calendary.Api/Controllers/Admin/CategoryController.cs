using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;


[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/category")]
public class CategoryController : BaseAdminController
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public CategoryController(IUserService userService, ICategoryService categoryService, IMapper mapper)
            : base(userService)
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        var result = _mapper.Map<List<CategoryDto>>(categories);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        var result = _mapper.Map<CategoryDto>(category);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = _mapper.Map<Category>(categoryDto);
        await _categoryService.CreateAsync(category);
        var createdCategory = await _categoryService.GetByIdAsync(category.Id);
        var result = _mapper.Map<CategoryDto>(createdCategory);
        return CreatedAtAction(nameof(Get), new { id = category.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingCategory = await _categoryService.GetByIdAsync(categoryDto.Id);
        if (existingCategory == null)
        {
            return NotFound();
        }

        var category = _mapper.Map<Category>(categoryDto);
        await _categoryService.UpdateAsync(category);
        var updatedCategory = await _categoryService.GetByIdAsync(category.Id);
        var result = _mapper.Map<CategoryDto>(updatedCategory);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingCategory = await _categoryService.GetByIdAsync(id);
        if (existingCategory == null)
        {
            return NotFound();
        }

        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
