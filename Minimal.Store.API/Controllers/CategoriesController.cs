using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Interfaces;

namespace Minimal.Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var categoryDto = await _categoryService.CreateAsync(dto);
        return CreatedAtAction("GetById", new { id = categoryDto.Id }, categoryDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
    {
        var updatedCategory = await _categoryService.UpdateAsync(id, dto);

        if (updatedCategory == null)
            return NotFound();

        return Ok(updatedCategory);
    }
}