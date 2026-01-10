using RagWorker.Models.Rag;

namespace RagWorker.Interfaces.Rag;

public interface IRagQueryProcessor
{
    Task<RagResult> ProcessAsync(
        RagQuery query,
        CancellationToken cancellationToken);
}