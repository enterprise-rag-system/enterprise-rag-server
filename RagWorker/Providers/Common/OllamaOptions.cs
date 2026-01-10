namespace RagWorker.Providers.Common;

public class OllamaOptions
{
    /// <summary>
    /// Ollama base URL (usually http://localhost:11434)
    /// </summary>
    public string BaseUrl { get; set; } = default!;

    /// <summary>
    /// Chat model name (e.g. mistral, llama3)
    /// </summary>
    public string ChatModel { get; set; } = default!;

    /// <summary>
    /// Embedding model name (e.g. nomic-embed-text)
    /// </summary>
    public string EmbeddingModel { get; set; } = default!;
}