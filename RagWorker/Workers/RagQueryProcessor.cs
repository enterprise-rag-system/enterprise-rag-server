using RagWorker.Interfaces.AI;
using RagWorker.Interfaces.Rag;
using RagWorker.Interfaces.Vector;
using RagWorker.Models.Rag;
using RagWorker.Models.AI;
using RagWorker.Helpers;

namespace RagWorker.Workers;

public sealed class RagQueryProcessor : IRagQueryProcessor
{
    private const int DefaultTopK = 5;

    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;
    private readonly IChatCompletionProvider _chatCompletionProvider;

    public RagQueryProcessor(
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore,
        IChatCompletionProvider chatCompletionProvider)
    {
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _chatCompletionProvider = chatCompletionProvider;
    }

    public async Task<RagResult> ProcessAsync(
        RagQuery query,
        CancellationToken cancellationToken)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        if (query.ProjectId == Guid.Empty)
            throw new ArgumentException("ProjectId is required");

        if (string.IsNullOrWhiteSpace(query.Question))
            throw new ArgumentException("Question cannot be empty");

        // 1️⃣ Generate embedding for the user question
        var rawEmbedding =
            await _embeddingProvider.GenerateEmbeddingAsync(
                query.Question,
                cancellationToken);

        // 2️⃣ Normalize embedding (CRITICAL for cosine similarity)
        var normalizedEmbedding =
            EmbeddingNormalizer.Normalize(rawEmbedding);

        // 3️⃣ Vector similarity search (project-scoped)
        var chunks =
            await _vectorStore.SearchAsync(
                query.ProjectId,
                normalizedEmbedding,
                DefaultTopK,
                cancellationToken);

        // 4️⃣ Build strict RAG prompt (single source of truth)
        var prompt =
            PromptBuilder.Build(
                query.Question,
                chunks.Select(c => c.ChunkText).ToList());

        // 5️⃣ Call LLM with prepared prompt
        var completion =
            await _chatCompletionProvider.CompleteAsync(
                new ChatCompletionRequest
                {
                    Prompt = prompt
                },
                cancellationToken);

        // 6️⃣ Return deterministic, auditable result
        return new RagResult
        {
            Answer = completion.Answer,
            SourceChunkIds = chunks.Select(c => c.Id).ToList(),
            TotalTokens =
                completion.PromptTokens +
                completion.CompletionTokens
        };
    }
}
