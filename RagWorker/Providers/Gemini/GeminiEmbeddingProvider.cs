using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RagWorker.Helpers;
using RagWorker.Interfaces.AI;
using RagWorker.Providers.Common;

namespace RagWorker.Providers.Gemini;

public class GeminiEmbeddingProvider: IEmbeddingProvider
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;
    private readonly AiProviderOptions _providerOptions;
    private readonly ILogger<GeminiEmbeddingProvider> _logger;

    public GeminiEmbeddingProvider(
        HttpClient http,
        IOptions<GeminiOptions> options,
        IOptions<AiProviderOptions> providerOptions,
        ILogger<GeminiEmbeddingProvider> logger)
    {
        _http = http;
        _options = options.Value;
        _providerOptions = providerOptions.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input text is empty");

        return await RetryPolicyHelper.ExecuteAsync(
            async () =>
            {
                _logger.LogDebug("Generating embedding using Gemini");

                var request = new GeminiEmbeddingRequest
                {
                    Content = new GeminiEmbeddingContent
                    {
                        Parts = [ new GeminiEmbeddingPart { Text = input } ]
                    }
                };

                var response = await _http.PostAsJsonAsync(
                    $"{_options.BaseUrl}/models/{_options.EmbeddingModel}:embedContent?key={_options.ApiKey}",
                    request,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                using var json = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);

                var embedding = json.RootElement
                    .GetProperty("embedding")
                    .GetProperty("values")
                    .EnumerateArray()
                    .Select(x => x.GetSingle())
                    .ToList();

                if (embedding.Count == 0)
                    throw new InvalidOperationException("Gemini returned empty embedding");

                _logger.LogDebug(
                    "Embedding generated with {Dimension} dimensions",
                    embedding.Count);

                return embedding;
            },
            _providerOptions.MaxRetries,
            _providerOptions.RetryDelayMs);
    }
    
    // ---------- Gemini DTOs (provider-only) ----------

    private sealed class GeminiEmbeddingRequest
    {
        public GeminiEmbeddingContent Content { get; set; } = default!;
    }

    private sealed class GeminiEmbeddingContent
    {
        public List<GeminiEmbeddingPart> Parts { get; set; } = [];
    }

    private sealed class GeminiEmbeddingPart
    {
        public string Text { get; set; } = default!;
    }
}