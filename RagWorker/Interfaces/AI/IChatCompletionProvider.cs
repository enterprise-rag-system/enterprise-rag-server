using RagWorker.Models.AI;

namespace RagWorker.Interfaces.AI;

public interface IChatCompletionProvider
{
    Task<ChatCompletionResult> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken);
}   