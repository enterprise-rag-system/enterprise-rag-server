using RagWorker.Interfaces.AI;

namespace RagWorker.Providers.Gemini;

public class GeminiOptions: IAiProviderConnectionOptions, IChatModelOptions, IEmbeddingModelOptions
{
    public string ApiKey { get; set; } = default!;
    public string BaseUrl { get; set; } = default;

    public string ChatModel { get; set; } = "models/gemini-1.5-flash";
    public string EmbeddingModel { get; set; } = "models/text-embedding-004";
}