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

    [Fact]
    public async Task GetProductById_WithValidId_ShouldReturnProductWithCategoryInformation()
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

        // 新增測試商品
        var product = new Product
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 100,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        // Act
        var result = await controller.GetById(product.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedProduct = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        returnedProduct.Id.Should().Be(product.Id);
        returnedProduct.Name.Should().Be("iPhone 15");
        returnedProduct.Description.Should().Be("Latest iPhone model");
        returnedProduct.Price.Should().Be(999.99m);
        returnedProduct.Stock.Should().Be(100);
        returnedProduct.CategoryId.Should().Be(category.Id);
        returnedProduct.CategoryName.Should().Be("Electronics");
        returnedProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var productRepository = new ProductRepository(Context);
        var productService = new ProductService(productRepository);
        var controller = new ProductsController(productService);

        // Act
        var result = await controller.GetById(999); // 不存在的ID

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldReturnUpdatedProductWithCategoryInformation()
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
        var product = new Product
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 100,
            CategoryId = category1.Id,
            CreatedAt = DateTime.UtcNow
        };

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var updateProductDto = new UpdateProductDto
        {
            Name = "iPhone 15 Pro",
            Description = "Latest iPhone Pro model with enhanced features",
            Price = 1099.99m,
            Stock = 80,
            CategoryId = category2.Id // 變更分類
        };

        // Act
        var result = await controller.Update(product.Id, updateProductDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var updatedProduct = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        updatedProduct.Id.Should().Be(product.Id);
        updatedProduct.Name.Should().Be("iPhone 15 Pro");
        updatedProduct.Description.Should().Be("Latest iPhone Pro model with enhanced features");
        updatedProduct.Price.Should().Be(1099.99m);
        updatedProduct.Stock.Should().Be(80);
        updatedProduct.CategoryId.Should().Be(category2.Id);
        updatedProduct.CategoryName.Should().Be("Clothing");
        updatedProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var productRepository = new ProductRepository(Context);
        var productService = new ProductService(productRepository);
        var controller = new ProductsController(productService);

        var updateProductDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated description",
            Price = 100.00m,
            Stock = 50,
            CategoryId = 1
        };

        // Act
        var result = await controller.Update(999, updateProductDto); // 不存在的ID

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldReturn204()
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

        // 新增測試商品
        var product = new Product
        {
            Name = "iPhone 15",
            Description = "Latest iPhone model",
            Price = 999.99m,
            Stock = 100,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        // Act
        var result = await controller.Delete(product.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // 驗證商品已被刪除
        var deletedProduct = await Context.Products.FindAsync(product.Id);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var productRepository = new ProductRepository(Context);
        var productService = new ProductService(productRepository);
        var controller = new ProductsController(productService);

        // Act
        var result = await controller.Delete(999); // 不存在的ID

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}