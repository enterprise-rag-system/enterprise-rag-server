using AuthService.Entities;

namespace AuthService.Interfaces;

public interface IJwtTokenService
{
    string GenerateJwtToken(User user);
}