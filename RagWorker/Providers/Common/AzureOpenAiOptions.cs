namespace RagWorker.Providers.Common;

public class AzureOpenAiOptions
{
    /// <summary>
    /// Azure OpenAI endpoint
    /// </summary>
    public string Endpoint { get; set; } = default!;

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string ApiKey { get; set; } = default!;

    /// <summary>
    /// Chat completion deployment name
    /// </summary>
    public string ChatDeployment { get; set; } = default!;

    /// <summary>
    /// Embedding deployment name
    /// </summary>
    public string EmbeddingDeployment { get; set; } = default!;
}