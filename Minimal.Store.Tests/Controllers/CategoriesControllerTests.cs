using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Controllers;
using Minimal.Store.API.Data.Repositories;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Models.Entities;
using Minimal.Store.API.Services.Implementations;

namespace Minimal.Store.Tests.Controllers;

public class CategoriesControllerTests : TestBase
{
    [Fact]
    public async Task CreateCategory_WithValidData_ShouldReturn201()
    {
        // Arrange
        var categoryRepository = new CategoryRepository(Context);
        var categoryService = new CategoryService(categoryRepository);
        var controller = new CategoriesController(categoryService);
        var createCategoryDto = new CreateCategoryDto
        {
            Name = "Electronics",
            Description = "Electronic products and gadgets"
        };

        // Act
        var result = await controller.Create(createCategoryDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var createdCategory = createdResult.Value.Should().BeOfType<CategoryDto>().Subject;
        createdCategory.Id.Should().BeGreaterThan(0);
        createdCategory.Name.Should().Be("Electronics");
        createdCategory.Description.Should().Be("Electronic products and gadgets");
        createdCategory.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnCategoryList()
    {
        // Arrange
        var categoryRepository = new CategoryRepository(Context);
        var categoryService = new CategoryService(categoryRepository);
        var controller = new CategoriesController(categoryService);

        // 先新增一些測試資料
        var category1 = new Category { Name = "Electronics", Description = "Electronic products", CreatedAt = DateTime.UtcNow };
        var category2 = new Category { Name = "Books", Description = "Books and literature", CreatedAt = DateTime.UtcNow };

        Context.Categories.AddRange(category1, category2);
        await Context.SaveChangesAsync();

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryDto>>().Subject;
        categories.Should().HaveCount(2);

        var categoriesList = categories.ToList();
        categoriesList[0].Name.Should().Be("Electronics");
        categoriesList[0].Description.Should().Be("Electronic products");
        categoriesList[1].Name.Should().Be("Books");
        categoriesList[1].Description.Should().Be("Books and literature");
    }

    [Fact]
    public async Task GetCategoryById_WithValidId_ShouldReturnCategory()
    {
        // Arrange
        var categoryRepository = new CategoryRepository(Context);
        var categoryService = new CategoryService(categoryRepository);
        var controller = new CategoriesController(categoryService);

        // 先新增測試資料
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            CreatedAt = DateTime.UtcNow
        };

        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        // Act
        var result = await controller.GetById(category.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedCategory = okResult.Value.Should().BeOfType<CategoryDto>().Subject;
        returnedCategory.Id.Should().Be(category.Id);
        returnedCategory.Name.Should().Be("Electronics");
        returnedCategory.Description.Should().Be("Electronic products");
        returnedCategory.CreatedAt.Should().BeCloseTo(category.CreatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetCategoryById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var categoryRepository = new CategoryRepository(Context);
        var categoryService = new CategoryService(categoryRepository);
        var controller = new CategoriesController(categoryService);

        // Act
        var result = await controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}