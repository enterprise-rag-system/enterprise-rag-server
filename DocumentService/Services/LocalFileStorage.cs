using DocumentService.Infrastructure;

namespace DocumentService.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly ILogger<LocalFileStorage> _logger;
    private readonly string _basePath;

    public LocalFileStorage(
        IConfiguration configuration,
        ILogger<LocalFileStorage> logger)
    {
        _logger = logger;
        _basePath = configuration["Storage:UploadFolderPath"]
                    ?? "uploads";
            
    }

    public async Task<string> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);

        var filePath = Path.Combine(
            _basePath,
            $"{Guid.NewGuid()}_{file.FileName}");

        await using var stream = new FileStream(
            filePath, FileMode.Create);

        await file.CopyToAsync(stream, cancellationToken);

        _logger.LogInformation(
            "File saved at {Path}", filePath);

        return filePath;
    }
}
