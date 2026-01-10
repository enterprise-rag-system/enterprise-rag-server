using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Helpers;
using AuthService.Infrastructure;
using AuthService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class AuthService: IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext dbContext, IJwtTokenService jwtTokenService, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }


    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation("LoginAsync called");
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Username);
            
            if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("User created");
            
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();


            return new LoginResponseDto
            {
                AccessToken = _jwtTokenService.GenerateJwtToken(user),
                RefreshToken = refreshToken.Token,
                ExpiresIn = 3600
            };
            
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("LoginAsync Exception");
            throw;
        }
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            _logger.LogInformation("Register called");
            var userExists = _dbContext.Users.Any(u => u.Email == request.Email);

            if (userExists)
            {
                _logger.LogInformation("User already exists");
                throw new InvalidOperationException("User already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = PasswordHasher.Hash(request.Password),
                EmailConfirmed = false,
                PhoneConfirmed = false
            };
            
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("User created");
            
            return new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
            };

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("Register Exception");
            throw;
        }
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        try
        {
            _logger.LogInformation("RefreshTokenAsync called");
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.Token == request.RefreshToken &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow);

            if (token == null)
                throw new UnauthorizedAccessException("Invalid refresh token");

            var user = await _dbContext.Users.FindAsync(token.UserId);
            if (user == null)
                throw new UnauthorizedAccessException();

            // Revoke old token
            token.IsRevoked = true;

            // Issue new refresh token
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _dbContext.RefreshTokens.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = _jwtTokenService.GenerateJwtToken(user),
                RefreshToken = newRefreshToken.Token,
                ExpiresIn = 3600
            };

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}