using RagWorker.Interfaces.AI;

namespace RagWorker.Interfaces.Factory;

public interface IEmbeddingProviderFactory
{
    IEmbeddingProvider Create();
}