using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Minimal.Store.API.Controllers;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Interfaces;
using Minimal.Store.API.Services.Implementations;

namespace Minimal.Store.Tests.Controllers;

public class RefreshTokenControllerTests : TestBase
{
    private readonly AuthController _controller;

    public RefreshTokenControllerTests()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Context);

        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "your-secret-key-here-that-is-at-least-32-characters-long"},
            {"Jwt:Issuer", "test-app"},
            {"Jwt:Audience", "test-app-users"},
            {"RefreshToken:ExpiryDays", "7"}
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
    public async Task Register_ShouldReturnAuthResponseWithRefreshToken()
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
        response.RefreshToken.Should().NotBeEmpty();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldReturnAuthResponseWithRefreshToken()
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
        response.RefreshToken.Should().NotBeEmpty();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "refreshuser",
            Email = "refresh@example.com",
            Password = "RefreshPassword123!"
        };
        var registerResult = await _controller.Register(registerDto);
        var registerResponse = (registerResult as CreatedAtActionResult)?.Value as AuthResponseDto;

        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = registerResponse!.RefreshToken
        };

        var result = await _controller.RefreshToken(refreshRequest);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as TokenResponseDto;
        response.Should().NotBeNull();
        response!.UserName.Should().Be("refreshuser");
        response.Email.Should().Be("refresh@example.com");
        response.AccessToken.Should().NotBeEmpty();
        response.RefreshToken.Should().NotBeEmpty();
        response.RefreshToken.Should().NotBe(registerResponse.RefreshToken);
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturn401()
    {
        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = "invalid-refresh-token"
        };

        var result = await _controller.RefreshToken(refreshRequest);

        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RevokeToken_WithValidToken_ShouldReturn200()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "revokeuser",
            Email = "revoke@example.com",
            Password = "RevokePassword123!"
        };
        var registerResult = await _controller.Register(registerDto);
        var registerResponse = (registerResult as CreatedAtActionResult)?.Value as AuthResponseDto;

        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = registerResponse!.RefreshToken
        };

        var result = await _controller.RevokeToken(refreshRequest);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RevokeToken_WithInvalidToken_ShouldReturn401()
    {
        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = "invalid-refresh-token"
        };

        var result = await _controller.RevokeToken(refreshRequest);

        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ShouldReturn401()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "revokeduser",
            Email = "revoked@example.com",
            Password = "RevokedPassword123!"
        };
        var registerResult = await _controller.Register(registerDto);
        var registerResponse = (registerResult as CreatedAtActionResult)?.Value as AuthResponseDto;

        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = registerResponse!.RefreshToken
        };

        await _controller.RevokeToken(refreshRequest);

        var result = await _controller.RefreshToken(refreshRequest);

        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(401);
    }
}