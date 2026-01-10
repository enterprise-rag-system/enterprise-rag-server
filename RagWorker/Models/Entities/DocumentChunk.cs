
using Pgvector;

namespace RagWorker.Models.Entities;

public sealed class DocumentChunk
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Guid DocumentId { get; set; }

    public string ChunkText { get; set; } = null!;

    public Vector Embedding { get; set; } = null!;

    public int ChunkIndex { get; set; }
    public int TokenCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
