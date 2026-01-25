using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RagWorker.Helpers;
using RagWorker.Interfaces.AI;
using RagWorker.Models.AI;
using RagWorker.Providers.Common;

namespace RagWorker.Providers.Gemini;

public class GeminiChatCompletionProvider : IChatCompletionProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _geminiOptions;
    private readonly AiProviderOptions _providerOptions;
    private readonly ILogger<GeminiChatCompletionProvider> _logger;

    public GeminiChatCompletionProvider(
        HttpClient httpClient,
        IOptions<GeminiOptions> geminiOptions,
        IOptions<AiProviderOptions> providerOptions,
        ILogger<GeminiChatCompletionProvider> logger)
    {
        _httpClient = httpClient;
        _geminiOptions = geminiOptions.Value;
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
                _logger.LogDebug("Generating chat completion using Gemini");

                var geminiRequest = new GeminiChatRequest
                {
                    Contents =
                    [
                        new GeminiContent
                        {
                            Parts = [ new GeminiPart { Text = request.Prompt } ]
                        }
                    ]
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_geminiOptions.BaseUrl}/models/{_geminiOptions.ChatModel}:generateContent?key={_geminiOptions.ApiKey}",
                    geminiRequest,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                using var json = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);

                var answer = json.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(answer))
                    throw new InvalidOperationException("Empty response from Gemini");

                return new ChatCompletionResult
                {
                    Answer = answer.Trim(),
                    PromptTokens = 0,        // Gemini free tier does not expose tokens
                    CompletionTokens = 0
                };
            },
            _providerOptions.MaxRetries,
            _providerOptions.RetryDelayMs);
    }

    // ---------- Gemini DTOs (provider-only) ----------

    private sealed class GeminiChatRequest
    {
        public List<GeminiContent> Contents { get; set; } = [];
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiPart
    {
        public string Text { get; set; } = default!;
    }
}