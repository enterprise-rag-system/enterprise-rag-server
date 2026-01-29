using AuthService.Controllers;
using AuthService.DTOs;
using AuthService.Interfaces;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Test.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authService = A.Fake<IAuthService>();
        _controller = new AuthController(_authService);
    }

    [Fact]
    public async Task Login_Should_return_ok()
    {
        A.CallTo(() => _authService.LoginAsync(A<LoginRequestDto>._))
            .Returns(new LoginResponseDto { AccessToken = "token" });

        var result = await _controller.Login(new LoginRequestDto());

        result.Should().BeOfType<OkObjectResult>();
    }
}
