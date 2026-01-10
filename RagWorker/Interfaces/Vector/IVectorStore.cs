using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RagWorker.Models.Entities;

namespace RagWorker.Interfaces.Vector;

public interface IVectorStore
{
    Task StoreAsync(
        DocumentChunk chunk,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentChunk>> SearchAsync(
        Guid projectId,
        IReadOnlyList<float> embedding,
        int topK,
        CancellationToken cancellationToken);
}