using RagWorker.Interfaces.AI;

namespace RagWorker.Providers.Common;

public class AzureOpenAiOptions: IAiProviderConnectionOptions,
    IChatModelOptions,
    IEmbeddingModelOptions
{
    /// <summary>
    /// Azure OpenAI endpoint
    /// </summary>
    public string BaseUrl { get; }

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string ApiKey { get; set; } = default!;
    
    public string ChatModel { get; }
    
    public string EmbeddingModel { get; }
}