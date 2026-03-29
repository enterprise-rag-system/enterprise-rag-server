using DocumentService.Model.DTOs;
using DocumentService.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DocumentService.Filters;

public class DocumentValidationFilter:IAsyncActionFilter
{
    private readonly ILogger<DocumentValidationFilter> _logger;

    public DocumentValidationFilter(ILogger<DocumentValidationFilter> logger)
    {
        _logger = logger;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, string[]>();

        if (context.ActionArguments.TryGetValue("projectId", out var projectIdObj))
        {
            if (projectIdObj is Guid projectId && projectId == Guid.Empty)
            {
                errors["projectId"] = new[] { "Invalid projectId" };
            }
        }
        if (context.ActionDescriptor.DisplayName?.Contains("Upload") == true)
        {
            if (context.ActionArguments.TryGetValue("file", out var fileObj))
            {
                var file = fileObj as IFormFile;
                var fileErrors = FileValidationHelper.Validate(file);

                if (fileErrors.Any())
                {
                    errors["file"] = fileErrors.ToArray();
                }
            }
        }

        if (errors.Any())
        {
            _logger.LogWarning("Document validation failed");

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