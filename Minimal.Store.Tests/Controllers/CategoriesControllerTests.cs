using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Controllers;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Implementations;

namespace Minimal.Store.Tests.Controllers;

public class CategoriesControllerTests : TestBase
{
    [Fact]
    public async Task CreateCategory_WithValidData_ShouldReturn201()
    {
        // Arrange
        var categoryService = new CategoryService(Context);
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
}