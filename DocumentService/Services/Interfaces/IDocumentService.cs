using DocumentService.Model.DTOs;

namespace DocumentService.Services.Interfaces;

public interface IDocumentService
{
    Task<DocumentUploadResponseDto> UploadAsync(
        Guid projectId,
        IFormFile file,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentListDto>> GetByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken);

}