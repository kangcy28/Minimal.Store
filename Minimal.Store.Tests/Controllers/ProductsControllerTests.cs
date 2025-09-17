using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Controllers;
using Minimal.Store.API.Data.Repositories;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Models.Entities;
using Minimal.Store.API.Services.Implementations;

namespace Minimal.Store.Tests.Controllers;

public class ProductsControllerTests : TestBase
{
    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturn201()
    {
        // Arrange
        var productRepository = new ProductRepository(Context);
        var productService = new ProductService(productRepository);
        var controller = new ProductsController(productService);

        // 先新增測試分類
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            CreatedAt = DateTime.UtcNow
        };

        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var createProductDto = new CreateProductDto
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 100,
            CategoryId = category.Id
        };

        // Act
        var result = await controller.Create(createProductDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var createdProduct = createdResult.Value.Should().BeOfType<ProductDto>().Subject;
        createdProduct.Id.Should().BeGreaterThan(0);
        createdProduct.Name.Should().Be("iPhone 15");
        createdProduct.Description.Should().Be("Latest iPhone model");
        createdProduct.Price.Should().Be(999.99m);
        createdProduct.Stock.Should().Be(100);
        createdProduct.CategoryId.Should().Be(category.Id);
        createdProduct.CategoryName.Should().Be("Electronics");
        createdProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}