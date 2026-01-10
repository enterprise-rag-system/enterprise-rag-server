namespace RagWorker.Interfaces.AI;

public interface IEmbeddingProvider
{
    Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken);
}