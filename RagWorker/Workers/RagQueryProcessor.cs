using RagWorker.Interfaces.AI;
using RagWorker.Interfaces.Rag;
using RagWorker.Interfaces.Vector;
using RagWorker.Models.Rag;
using RagWorker.Models.AI;
using RagWorker.Helpers;
using RagWorker.Interfaces.Factory;
using RagWorker.Interfaces.Translator;

namespace RagWorker.Workers;

public sealed class RagQueryProcessor : IRagQueryProcessor
{
    private const int DefaultTopK = 5;

    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;
    private readonly IChatCompletionProvider _chatCompletionProvider;
    private readonly ITranslationService _translator;


    public RagQueryProcessor(
        IVectorStore vectorStore,
        IChatCompletionProviderFactory chatFactory,
        IEmbeddingProviderFactory embeddingFactory,
        ITranslationService translator)
    {
        _vectorStore = vectorStore;
        _translator = translator;
        _chatCompletionProvider = chatFactory.Create();
        _embeddingProvider = embeddingFactory.Create();
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
        
        var language = await _translator.DetectLanguageAsync(query.Question);
        
        var englishQuery = language == "en"
            ? query.Question
            : await _translator.TranslateToEnglishAsync(query.Question);
        
        // 1️⃣ Generate embedding for the user question
        var rawEmbedding =
            await _embeddingProvider.GenerateEmbeddingAsync(
                englishQuery,
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
        
        var finalResponse = language == "en"
            ? completion.Answer
            : await _translator.TranslateFromEnglishAsync(completion.Answer, language);

        
        // 6️⃣ Return deterministic, auditable result
        return new RagResult
        {
            Answer = finalResponse,
            SourceChunkIds = chunks.Select(c => c.Id).ToList(),
            TotalTokens =
                completion.PromptTokens +
                completion.CompletionTokens
        };
    }
}
