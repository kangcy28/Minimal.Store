using Minimal.Store.API.Models.DTOs;

namespace Minimal.Store.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginUserDto loginDto);
    Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}