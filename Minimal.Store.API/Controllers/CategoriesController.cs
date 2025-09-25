using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Interfaces;

namespace Minimal.Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        _logger.LogInformation("Creating category with name: {CategoryName}", dto.Name);
        var categoryDto = await _categoryService.CreateAsync(dto);
        _logger.LogInformation("Successfully created category with ID: {CategoryId}", categoryDto.Id);
        return CreatedAtAction("GetById", new { id = categoryDto.Id }, categoryDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Retrieving all categories");
        var categories = await _categoryService.GetAllAsync();
        _logger.LogInformation("Retrieved {CategoryCount} categories", categories.Count());
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Retrieving category with ID: {CategoryId}", id);
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
        {
            _logger.LogWarning("Category with ID: {CategoryId} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully retrieved category: {CategoryName}", category.Name);
        return Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
    {
        _logger.LogInformation("Updating category with ID: {CategoryId} to name: {CategoryName}", id, dto.Name);
        var updatedCategory = await _categoryService.UpdateAsync(id, dto);

        if (updatedCategory == null)
        {
            _logger.LogWarning("Category with ID: {CategoryId} not found for update", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully updated category with ID: {CategoryId}", id);
        return Ok(updatedCategory);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting category with ID: {CategoryId}", id);
        var result = await _categoryService.DeleteAsync(id);

        if (!result)
        {
            _logger.LogWarning("Category with ID: {CategoryId} not found for deletion", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully deleted category with ID: {CategoryId}", id);
        return NoContent();
    }
}