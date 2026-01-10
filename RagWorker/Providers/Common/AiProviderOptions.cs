namespace RagWorker.Providers.Common;

public class AiProviderOptions
{
    /// <summary>
    /// Selected provider (AzureOpenAI / Ollama)
    /// </summary>
    public string Provider { get; set; } = default!;

    /// <summary>
    /// Max retries for transient failures
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Retry delay in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 500;
}