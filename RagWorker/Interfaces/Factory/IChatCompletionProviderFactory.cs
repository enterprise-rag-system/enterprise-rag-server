using RagWorker.Interfaces.AI;

namespace RagWorker.Interfaces.Factory;

public interface IChatCompletionProviderFactory
{
    IChatCompletionProvider Create();
}