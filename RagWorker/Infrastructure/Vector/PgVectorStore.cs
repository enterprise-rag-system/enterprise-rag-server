using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using RagWorker.Infrastructure.Persistence;
using RagWorker.Interfaces.Vector;
using RagWorker.Models.Entities;

namespace RagWorker.Infrastructure.Vector;

public class PgVectorStore : IVectorStore
{
    private readonly ILogger<PgVectorStore> _logger;
    private readonly RagDbContext _dbContext;

    public PgVectorStore(
        ILogger<PgVectorStore> logger,
        RagDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task StoreAsync(
        DocumentChunk chunk,
        CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "Saving document chunk {ChunkId}", chunk.Id);

            _dbContext.DocumentChunks.Add(chunk);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Document chunk {ChunkId} saved", chunk.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error saving document chunk {ChunkId}",
                chunk.Id);
            throw;
        }
    }

    public async Task<IReadOnlyList<DocumentChunk>> SearchAsync(
        Guid projectId,
        IReadOnlyList<float> embedding,
        int topK,
        CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "Vector search for project {ProjectId}", projectId);

            var vector = new Pgvector.Vector(embedding.ToArray());

            var chunks = await _dbContext.DocumentChunks
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.Embedding.CosineDistance((vector)))
                .Take(topK)
                .ToListAsync(ct);
            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Vector search failed for project {ProjectId}",
                projectId);
            throw;
        }
    }
}
