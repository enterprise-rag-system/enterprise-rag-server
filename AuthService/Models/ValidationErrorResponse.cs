namespace AuthService.Models;

public class ValidationErrorResponse
{
    public string ErrorCode { get; set; } = "VALIDATION_ERROR";
    public string Message { get; set; } = "Validation failed";
    public string TraceId { get; set; } = string.Empty;
    public Dictionary<string, string[]> Errors { get; set; } = new();
}