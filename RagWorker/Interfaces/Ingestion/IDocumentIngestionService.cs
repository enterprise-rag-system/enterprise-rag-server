using Shared;

namespace RagWorker.Interfaces.Ingestion;

public interface IDocumentIngestionService
{
    Task IngestAsync(DocumentUploadedEvent evt, CancellationToken ct);

}