using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagWorker.Interfaces.AI;
using RagWorker.Models.AI;
using RagWorker.Helpers;
using RagWorker.Providers.Common;

namespace RagWorker.Providers.Ollama;

public sealed class OllamaChatCompletionProvider : IChatCompletionProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _ollamaOptions;
    private readonly AiProviderOptions _providerOptions;
    private readonly ILogger<OllamaChatCompletionProvider> _logger;

    public OllamaChatCompletionProvider(
        HttpClient httpClient,
        IOptions<OllamaOptions> ollamaOptions,
        IOptions<AiProviderOptions> providerOptions,
        ILogger<OllamaChatCompletionProvider> logger)
    {
        _httpClient = httpClient;
        _ollamaOptions = ollamaOptions.Value;
        _providerOptions = providerOptions.Value;
        _logger = logger;
    }

    public async Task<ChatCompletionResult> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt cannot be empty");

        return await RetryPolicyHelper.ExecuteAsync(
            async () =>
            {
                var ollamaRequest = new OllamaChatRequest
                {
                    Model = _ollamaOptions.ChatModel,
                    Prompt = request.Prompt,
                    Stream = false
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_ollamaOptions.BaseUrl}/api/generate",
                    ollamaRequest,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var result =
                    await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                        cancellationToken: cancellationToken);

                if (string.IsNullOrWhiteSpace(result?.Response))
                    throw new InvalidOperationException("Empty response from Ollama");

                return new ChatCompletionResult
                {
                    Answer = result.Response.Trim(),
                    PromptTokens = result.PromptEvalCount ?? 0,
                    CompletionTokens = result.EvalCount ?? 0
                };
            },
            _providerOptions.MaxRetries,
            _providerOptions.RetryDelayMs);
    }
    
    // ---------- Ollama DTOs (provider-only) ----------

    private sealed class OllamaChatRequest
    {
        public string Model { get; set; } = default!;
        public string Prompt { get; set; } = default!;
        public bool Stream { get; set; }
    }

    private sealed class OllamaChatResponse
    {
        public string Response { get; set; } = default!;
        public int? PromptEvalCount { get; set; }
        public int? EvalCount { get; set; }
    }
}
