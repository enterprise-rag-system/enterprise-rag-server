using AuthService.DTOs;
using FluentValidation;

namespace AuthService.Validators;

public class LogoutRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
