using AuthService.DTOs;
using FluentValidation;

namespace AuthService.Validators;

public class RegisterRequestValidator: AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must have at least 8 characters");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .MinimumLength(3).WithMessage("Username must have at least 3 characters");
    }
}