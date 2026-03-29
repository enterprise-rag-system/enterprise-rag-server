using ChatService.Models.DTOs;
using FluentValidation;

namespace ChatService.Validators;

public class AddChatMessageValidator: AbstractValidator<AddChatMessageRequest>
{
    public AddChatMessageValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message too long");

        // 🔥 AI Guardrail (basic)
        RuleFor(x => x.Message)
            .Must(BeSafeInput)
            .WithMessage("Invalid or unsafe input detected");
    }

    private bool BeSafeInput(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        var lower = message.ToLower();

        // Basic prompt injection patterns
        var blockedPatterns = new[]
        {
            "ignore previous instructions",
            "system prompt",
            "act as",
            "bypass",
            "override instructions"
        };

        return !blockedPatterns.Any(p => lower.Contains(p));
    }

}