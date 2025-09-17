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

    [Fact]
    public async Task GetAllProducts_ShouldIncludeCategoryInformation()
    {
        // Arrange
        var productRepository = new ProductRepository(Context);
        var productService = new ProductService(productRepository);
        var controller = new ProductsController(productService);

        // 先新增測試分類
        var category1 = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            CreatedAt = DateTime.UtcNow
        };

        var category2 = new Category
        {
            Name = "Clothing",
            Description = "Fashion items",
            CreatedAt = DateTime.UtcNow
        };

        Context.Categories.AddRange(category1, category2);
        await Context.SaveChangesAsync();

        // 新增測試商品
        var product1 = new Product
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 100,
            CategoryId = category1.Id,
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Name = "T-Shirt",
            Description = "Cotton T-Shirt",
            Price = 29.99m,
            Stock = 50,
            CategoryId = category2.Id,
            CreatedAt = DateTime.UtcNow
        };

        Context.Products.AddRange(product1, product2);
        await Context.SaveChangesAsync();

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var products = okResult.Value.Should().BeOfType<List<ProductDto>>().Subject;
        products.Should().HaveCount(2);

        // 驗證第一個商品包含分類資訊
        var firstProduct = products.First(p => p.Name == "iPhone 15");
        firstProduct.CategoryId.Should().Be(category1.Id);
        firstProduct.CategoryName.Should().Be("Electronics");

        // 驗證第二個商品包含分類資訊
        var secondProduct = products.First(p => p.Name == "T-Shirt");
        secondProduct.CategoryId.Should().Be(category2.Id);
        secondProduct.CategoryName.Should().Be("Clothing");
    }
}