using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Minimal.Store.API.Controllers;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Interfaces;
using Minimal.Store.API.Services.Implementations;

namespace Minimal.Store.Tests.Controllers;

public class AuthControllerTests : TestBase
{
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Context);

        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "your-secret-key-here-that-is-at-least-32-characters-long"},
            {"Jwt:Issuer", "test-app"},
            {"Jwt:Audience", "test-app-users"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<IAuthService, AuthService>();

        var serviceProvider = services.BuildServiceProvider();

        var authService = serviceProvider.GetService<IAuthService>();
        _controller = new AuthController(authService!);
    }

    [Fact]
    public async Task Register_ValidUser_ShouldReturn201()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var result = await _controller.Register(registerDto);

        var createdResult = result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);

        var response = createdResult.Value as AuthResponseDto;
        response.Should().NotBeNull();
        response!.UserName.Should().Be("testuser");
        response.Email.Should().Be("test@example.com");
        response.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldReturn400()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "testuser1",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        await _controller.Register(registerDto);

        var duplicateDto = new RegisterUserDto
        {
            UserName = "testuser2",
            Email = "duplicate@example.com",
            Password = "Password456!"
        };

        var result = await _controller.Register(duplicateDto);

        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturn200WithToken()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "loginuser",
            Email = "login@example.com",
            Password = "LoginPassword123!"
        };
        await _controller.Register(registerDto);

        var loginDto = new LoginUserDto
        {
            Email = "login@example.com",
            Password = "LoginPassword123!"
        };

        var result = await _controller.Login(loginDto);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as AuthResponseDto;
        response.Should().NotBeNull();
        response!.UserName.Should().Be("loginuser");
        response.Email.Should().Be("login@example.com");
        response.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturn401()
    {
        var loginDto = new LoginUserDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        var result = await _controller.Login(loginDto);

        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_WrongPassword_ShouldReturn401()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "passworduser",
            Email = "password@example.com",
            Password = "CorrectPassword123!"
        };
        await _controller.Register(registerDto);

        var loginDto = new LoginUserDto
        {
            Email = "password@example.com",
            Password = "WrongPassword123!"
        };

        var result = await _controller.Login(loginDto);

        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(401);
    }
}