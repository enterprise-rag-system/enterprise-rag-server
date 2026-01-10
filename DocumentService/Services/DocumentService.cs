using DocumentService.Infrastructure;
using DocumentService.Model.DTOs;
using DocumentService.Model.Entities;
using DocumentService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DocumentService.Services;

public class DocumentService: IDocumentService
{
    private readonly ILogger<DocumentService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IFileStorage _storage;
    private readonly IRabbitMqPublisher _publisher;
    
    

    public DocumentService(
        ILogger<DocumentService> logger, 
        AppDbContext dbContext, 
        IFileStorage storage,
        IRabbitMqPublisher publisher
        )
    {
        _logger = logger;
        _dbContext = dbContext;
        _storage = storage;
        _publisher = publisher;
    }
    
    public async Task<DocumentUploadResponseDto> UploadAsync(Guid projectId, IFormFile file, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Uploading file {fileName}", file.FileName);
            _logger.LogInformation("Doc upload request for project {ProjectId}", projectId);
            var path = await _storage.SaveAsync(file, cancellationToken);
            
            var document = new Document
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                Status = Status.Uploaded,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = userId
            };

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document uploaded. DocumentId={DocumentId}, ProjectId={ProjectId}",
                document.Id, projectId);
            //var absolutePath = Path.GetFullPath(path);

            _publisher.PublishMessage(
                new DocumentUploadedEvent(
                    projectId,
                    document.Id,
                    path
                ));

            return new DocumentUploadResponseDto
            {
                DocumentId = document.Id,
                Status = document.Status.ToString()
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("Failed to save document");
            throw;
        }
    }

    public async Task<IReadOnlyList<DocumentListDto>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting documents for project {ProjectId}", projectId);
            var documents = await _dbContext.Documents
                .Where(d => d.ProjectId == projectId)
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new DocumentListDto
                {
                    DocumentId = d.Id,
                    FileName = d.FileName,
                    Status = d.Status.ToString(),
                    UploadedAt = d.UploadedAt
                })
                .ToListAsync(cancellationToken);
            if (!documents.Any())
            {
                _logger.LogInformation("No documents found");
                return new List<DocumentListDto>();
            }
            _logger.LogInformation("Returning {Count} documents", documents.Count);
            return documents;
        }
        catch (Exception e)
        {
            
            _logger.LogError(e.Message);
            _logger.LogError(e.StackTrace);
            _logger.LogError("Failed to Fetch document");
            throw;
        }
    }
}