using AuthService.Services;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Helpers;
using AuthService.Infrastructure;
using AuthService.Interfaces;

using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AuthService.Test.Services;

public class AuthServiceTests
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtService;
    private readonly AuthService.Services.AuthService _authService;
    private ILogger<AuthService.Services.AuthService> _logger;
    
    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        _jwtService = A.Fake<IJwtTokenService>();
        _logger = A.Fake<ILogger<AuthService.Services.AuthService>>();

        _authService = new AuthService.Services.AuthService(
            _db,
            _jwtService,
            _logger
        );
    }
    
    [Fact]
    public async Task RegisterAsync_Should_create_user_when_email_not_exists()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@test.com",
            Password = "Password@123"
        };

        var response = await _authService.RegisterAsync(request);

        response.Should().NotBeNull();
        response.Email.Should().Be(request.Email);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_Should_throw_when_email_exists()
    {
        _db.Users.Add(new User
        {
            Email = "test@test.com",
            PasswordHash = "hash"
        });
        await _db.SaveChangesAsync();

        var request = new RegisterRequestDto
        {
            Email = "test@test.com",
            Password = "Password@123"
        };

        await FluentActions
            .Invoking(() => _authService.RegisterAsync(request))
            .Should()
            .ThrowAsync<Exception>(); // replace with custom exception if you have one
    }
    [Fact]
    public async Task LoginAsync_Should_return_token_when_credentials_valid()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = PasswordHasher.Hash("Password@123")
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        A.CallTo(() => _jwtService.GenerateJwtToken(user))
            .Returns("jwt-token");

        var request = new LoginRequestDto
        {
            Username = "test@test.com",
            Password = "Password@123"
        };

        var response = await _authService.LoginAsync(request);

        response.AccessToken.Should().Be("jwt-token");
    }
    [Fact]
    public async Task LoginAsync_Should_throw_when_password_invalid()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = PasswordHasher.Hash("CorrectPassword")
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "test@test.com",
            Password = "WrongPassword"
        };

        await FluentActions
            .Invoking(() => _authService.LoginAsync(request))
            .Should()
            .ThrowAsync<Exception>();
    }
    [Fact]
    public async Task RefreshTokenAsync_Should_return_new_token_when_valid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        var refreshToken = new RefreshToken
        {
            Token = "refresh-token",
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _db.Users.Add(user);
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        A.CallTo(() => _jwtService.GenerateJwtToken(user))
            .Returns("new-jwt");

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "refresh-token"
        };

        var response = await _authService.RefreshTokenAsync(request);

        response.AccessToken.Should().Be("new-jwt");
    }

}