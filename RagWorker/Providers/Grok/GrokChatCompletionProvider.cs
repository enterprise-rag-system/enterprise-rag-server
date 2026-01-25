using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagWorker.Helpers;
using RagWorker.Interfaces.AI;
using RagWorker.Models.AI;
using RagWorker.Providers.Common;

namespace RagWorker.Providers.Grok;

public sealed class GrokChatCompletionProvider : IChatCompletionProvider
{
    private readonly HttpClient _httpClient;
    private readonly GrokOptions _grokOptions;
    private readonly AiProviderOptions _providerOptions;
    private readonly ILogger<GrokChatCompletionProvider> _logger;

    public GrokChatCompletionProvider(
        HttpClient httpClient,
        IOptions<GrokOptions> grokOptions,
        IOptions<AiProviderOptions> providerOptions,
        ILogger<GrokChatCompletionProvider> logger)
    {
        _httpClient = httpClient;
        _grokOptions = grokOptions.Value;
        _providerOptions = providerOptions.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_grokOptions.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _grokOptions.ApiKey);
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
                _logger.LogDebug("Generating chat completion using Grok");

                var grokRequest = new GrokChatRequest
                {
                    Model = _grokOptions.ChatModel,
                    Messages =
                    [
                        new GrokMessage
                        {
                            Role = "user",
                            Content = request.Prompt
                        }
                    ]
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/v1/chat/completions",
                    grokRequest,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                using var json = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);

                var answer = json.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(answer))
                    throw new InvalidOperationException("Empty response from Grok");

                return new ChatCompletionResult
                {
                    Answer = answer.Trim(),
                    PromptTokens = 0,      // Grok does not expose token counts yet
                    CompletionTokens = 0
                };
            },
            _providerOptions.MaxRetries,
            _providerOptions.RetryDelayMs);
    }

    // ---------- Grok DTOs (provider-only) ----------

    private sealed class GrokChatRequest
    {
        public string Model { get; set; } = default!;
        public List<GrokMessage> Messages { get; set; } = [];
    }

    private sealed class GrokMessage
    {
        public string Role { get; set; } = default!;
        public string Content { get; set; } = default!;
    }
}
