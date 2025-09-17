using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Interfaces;

namespace Minimal.Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        var updatedProduct = await _productService.UpdateAsync(id, dto);

        if (updatedProduct == null)
            return NotFound();

        return Ok(updatedProduct);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var productDto = await _productService.CreateAsync(dto);
        return CreatedAtAction("GetById", new { id = productDto.Id }, productDto);
    }
}