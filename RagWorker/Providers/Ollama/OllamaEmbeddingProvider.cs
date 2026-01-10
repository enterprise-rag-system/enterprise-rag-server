using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RagWorker.Interfaces.AI;
using RagWorker.Helpers;
using RagWorker.Providers.Common;

namespace RagWorker.Providers.Ollama;

public sealed class OllamaEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _ollamaOptions;
    private readonly AiProviderOptions _providerOptions;
    private readonly ILogger<OllamaEmbeddingProvider> _logger;

    public OllamaEmbeddingProvider(
        HttpClient httpClient,
        IOptions<OllamaOptions> ollamaOptions,
        IOptions<AiProviderOptions> providerOptions,
        ILogger<OllamaEmbeddingProvider> logger)
    {
        _httpClient = httpClient;
        _ollamaOptions = ollamaOptions.Value;
        _providerOptions = providerOptions.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input text is empty");

        return await RetryPolicyHelper.ExecuteAsync(
            async () =>
            {
                _logger.LogDebug("Generating embedding using Ollama");

                var request = new OllamaEmbeddingRequest
                {
                    Model = _ollamaOptions.EmbeddingModel,
                    Prompt = input
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_ollamaOptions.BaseUrl}/api/embeddings",
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result =
                    await response.Content
                        .ReadFromJsonAsync<OllamaEmbeddingResponse>(
                            cancellationToken: cancellationToken);

                if (result?.Embedding == null || result.Embedding.Count == 0)
                    throw new InvalidOperationException(
                        "Ollama returned empty embedding");

                _logger.LogDebug(
                    "Embedding generated with {Dimension} dimensions",
                    result.Embedding.Count);

                return result.Embedding;
            },
            _providerOptions.MaxRetries,
            _providerOptions.RetryDelayMs);
    }

    // ---------- Ollama DTOs (private, provider-only) ----------

    private sealed class OllamaEmbeddingRequest
    {
        public string Model { get; set; } = default!;
        public string Prompt { get; set; } = default!;
    }

    private sealed class OllamaEmbeddingResponse
    {
        public List<float> Embedding { get; set; } = new();
    }
}
