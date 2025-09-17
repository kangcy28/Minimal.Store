using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Minimal.Store.API.Data.Context;
using Minimal.Store.API.Models.Entities;

namespace Minimal.Store.Tests;

public class DatabaseConnectionTest : TestBase
{
    [Fact]
    public void Context_Should_Connect_Successfully()
    {
        // Arrange & Act
        var canConnect = Context.Database.CanConnect();

        // Assert
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Context_Should_Create_And_Query_Category()
    {
        // Arrange
        var category = new Category
        {
            Name = "Test Category",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var savedCategory = await Context.Categories.FirstOrDefaultAsync();

        // Assert
        savedCategory.Should().NotBeNull();
        savedCategory!.Name.Should().Be("Test Category");
        savedCategory.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task Context_Should_Handle_Product_Category_Relationship()
    {
        // Arrange
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            CreatedAt = DateTime.UtcNow
        };

        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Laptop",
            Description = "Gaming laptop",
            Price = 999.99m,
            Stock = 10,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var savedProduct = await Context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync();

        // Assert
        savedProduct.Should().NotBeNull();
        savedProduct!.Category.Should().NotBeNull();
        savedProduct.Category.Name.Should().Be("Electronics");
    }
}