namespace RagWorker.Providers.Common;

public class AiProviderOptions
{
    /// <summary>
    /// Selected provider (AzureOpenAI / Ollama)
    /// </summary>
    public string ChatProvider { get; set; } = default!;
    
    public string EmbeddingProvider { get; set; } = default!;

    /// <summary>
    /// Max retries for transient failures
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Retry delay in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 500;
}