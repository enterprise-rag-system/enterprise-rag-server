using AuthService.DTOs;

namespace AuthService.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);


}