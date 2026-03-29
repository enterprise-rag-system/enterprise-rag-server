using ChatService.Models.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChatService.Filters;

public class ValidationFilter: IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate projectId from route
        if (context.ActionArguments.TryGetValue("projectId", out var projectIdObj))
        {
            if (projectIdObj is Guid projectId && projectId == Guid.Empty)
            {
                context.Result = new BadRequestObjectResult(new ValidationErrorResponse
                {
                    TraceId = context.HttpContext.TraceIdentifier,
                    Errors = new Dictionary<string, string[]>
                    {
                        { "projectId", new[] { "Invalid projectId" } }
                    }
                });
                return;
            }
        }
        foreach (var arg in context.ActionArguments)
        {
            if (arg.Value == null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.Value.GetType());
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                var validationContext = new ValidationContext<object>(arg.Value);
                ValidationResult result = await validator.ValidateAsync(validationContext);

                if (!result.IsValid)
                {
                    errors[arg.Key] = result.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray();
                }
            }
        }

        if (errors.Any())
        {
            var response = new ValidationErrorResponse
            {
                TraceId = context.HttpContext.TraceIdentifier,
                Errors = errors
            };

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}