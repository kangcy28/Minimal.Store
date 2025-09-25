using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Interfaces;

namespace Minimal.Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Retrieving products");
        var products = await _productService.GetAllAsync();
        _logger.LogInformation("Retrieved {ProductCount} products", products.Count());
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Product with ID: {ProductId} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully retrieved product: {ProductName}", product.Name);
        return Ok(product);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product with ID: {ProductId} to name: {ProductName}", id, dto.Name);
        var updatedProduct = await _productService.UpdateAsync(id, dto);

        if (updatedProduct == null)
        {
            _logger.LogWarning("Product with ID: {ProductId} not found for update", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully updated product with ID: {ProductId}", id);
        return Ok(updatedProduct);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);
        var result = await _productService.DeleteAsync(id);

        if (!result)
        {
            _logger.LogWarning("Product with ID: {ProductId} not found for deletion", id);
            return NotFound();
        }

        _logger.LogInformation("Successfully deleted product with ID: {ProductId}", id);
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        _logger.LogInformation("Creating product with name: {ProductName}, price: {ProductPrice}", dto.Name, dto.Price);
        var productDto = await _productService.CreateAsync(dto);
        _logger.LogInformation("Successfully created product with ID: {ProductId}", productDto.Id);
        return CreatedAtAction("GetById", new { id = productDto.Id }, productDto);
    }
}