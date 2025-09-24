using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Minimal.Store.API.Models.DTOs;

namespace Minimal.Store.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ShouldReturn401()
    {
        var product = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            Stock = 100,
            CategoryId = 1
        };

        var json = JsonSerializer.Serialize(product);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/products", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithValidToken_ShouldReturn201()
    {
        var token = await GetValidJwtTokenAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var product = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            Stock = 100,
            CategoryId = 1
        };

        var json = JsonSerializer.Serialize(product);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/products", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetProducts_WithoutAuth_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> GetValidJwtTokenAsync()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "integrationtest",
            Email = "integration@example.com",
            Password = "IntegrationTest123!"
        };

        var json = JsonSerializer.Serialize(registerDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return authResponse!.Token;
    }
}