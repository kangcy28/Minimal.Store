using Microsoft.AspNetCore.Mvc;
using Minimal.Store.API.Models.DTOs;
using Minimal.Store.API.Services.Interfaces;

namespace Minimal.Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerDto);
            return CreatedAtAction(nameof(Register), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequest)
    {
        var result = await _authService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        return Ok(result);
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto refreshTokenRequest)
    {
        var success = await _authService.RevokeTokenAsync(refreshTokenRequest.RefreshToken);

        if (!success)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        return Ok(new { message = "Token revoked successfully" });
    }
}