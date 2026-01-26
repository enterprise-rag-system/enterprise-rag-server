using Pgvector;
using RagWorker.Helpers;
using RagWorker.Infrastructure.Persistence;
using RagWorker.Interfaces.AI;
using RagWorker.Interfaces.Factory;
using RagWorker.Interfaces.Ingestion;
using RagWorker.Interfaces.Vector;
using RagWorker.Models.Entities;
using Shared;

namespace RagService.Services;

public class DocumentIngestionService: IDocumentIngestionService
{
    private readonly ITextExtractor _textExtractor;
    private readonly IChunkingService _chunker;
    private readonly IEmbeddingProvider _embedding;
    private readonly IVectorStore _vectorStore;
    private readonly RagDbContext _db;
    private readonly ILogger<DocumentIngestionService> _logger;

    public DocumentIngestionService(
        IChunkingService chunker,
        IEmbeddingProviderFactory factory,
        IVectorStore vectorStore,
        RagDbContext db,
        ITextExtractor textExtractor,
        ILogger<DocumentIngestionService> logger)
    {
        _chunker = chunker;
        _embedding = factory.Create();
        _vectorStore = vectorStore;
        _db = db;
        _logger = logger;
        _textExtractor = textExtractor;
    }

    public async Task IngestAsync(
        DocumentUploadedEvent evt,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Starting ingestion for DocumentId {DocumentId}",
            evt.DocumentId);

        var text = await _textExtractor.ExtractAsync(
            evt.FilePath,
            ct);

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning(
                "Extracted empty text for DocumentId {DocumentId}",
                evt.DocumentId);
            return;
        }

        var chunks = _chunker.Chunk(text);

        var index = 0;
        
        
        foreach (var (chunkText, tokenCount) in chunks)
        {
            var rawEmbedding =
                await _embedding.GenerateEmbeddingAsync(chunkText, ct);
            
            var embedding = EmbeddingNormalizer.ToVector(rawEmbedding);

            var entity = new DocumentChunk
            {
                Id = Guid.NewGuid(),
                ProjectId = evt.ProjectId,
                DocumentId = evt.DocumentId,
                ChunkText = chunkText,
                ChunkIndex = index++,
                TokenCount = tokenCount,
                Embedding = embedding
            };

            await _vectorStore.StoreAsync(entity, ct);
        }
        
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Document ingestion completed for DocumentId {DocumentId}",
            evt.DocumentId);
    }
}
