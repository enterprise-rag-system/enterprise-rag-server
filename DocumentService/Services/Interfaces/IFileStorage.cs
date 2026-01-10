namespace DocumentService.Infrastructure;

public interface IFileStorage
{
    Task<string> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken);

}