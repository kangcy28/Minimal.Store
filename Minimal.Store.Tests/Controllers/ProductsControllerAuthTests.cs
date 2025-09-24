using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Minimal.Store.API.Controllers;

namespace Minimal.Store.Tests.Controllers;

public class ProductsControllerAuthTests
{
    [Fact]
    public void CreateProduct_ShouldHaveAuthorizeAttribute()
    {
        var method = typeof(ProductsController).GetMethod("Create");
        var attributes = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        attributes.Should().HaveCount(1);
        attributes[0].Should().BeOfType<AuthorizeAttribute>();
    }

    [Fact]
    public void UpdateProduct_ShouldHaveAuthorizeAttribute()
    {
        var method = typeof(ProductsController).GetMethod("Update");
        var attributes = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        attributes.Should().HaveCount(1);
        attributes[0].Should().BeOfType<AuthorizeAttribute>();
    }

    [Fact]
    public void DeleteProduct_ShouldHaveAuthorizeAttribute()
    {
        var method = typeof(ProductsController).GetMethod("Delete");
        var attributes = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        attributes.Should().HaveCount(1);
        attributes[0].Should().BeOfType<AuthorizeAttribute>();
    }

    [Fact]
    public void GetAllProducts_ShouldNotHaveAuthorizeAttribute()
    {
        var method = typeof(ProductsController).GetMethod("GetAll");
        var attributes = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        attributes.Should().BeEmpty();
    }

    [Fact]
    public void GetByIdProduct_ShouldNotHaveAuthorizeAttribute()
    {
        var method = typeof(ProductsController).GetMethod("GetById");
        var attributes = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        attributes.Should().BeEmpty();
    }
}