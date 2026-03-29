namespace DocumentService.Validators;

public class FileValidationHelper
{
    private static readonly string[] AllowedExtensions =
    {
        ".pdf", ".doc", ".docx", ".txt"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public static List<string> Validate(IFormFile file)
    {
        var errors = new List<string>();

        if (file == null)
        {
            errors.Add("File is required");
            return errors;
        }

        if (file.Length == 0)
        {
            errors.Add("File cannot be empty");
        }

        if (file.Length > MaxFileSize)
        {
            errors.Add("File size must be less than 10MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!AllowedExtensions.Contains(extension))
        {
            errors.Add("Invalid file type. Allowed: PDF, DOC, DOCX, TXT");
        }

        return errors;
    }
}