using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using RagWorker.Interfaces.AI;
using RagWorker.Models.AI;
using RagWorker.Helpers;
using RagWorker.Providers.Common;

namespace RagWorker.Providers.Azure;

public sealed class AzureChatCompletionProvider : IChatCompletionProvider
{
    private readonly OpenAIClient _client;
    private readonly AzureOpenAiOptions _options;
    private readonly AiProviderOptions _providerOptions;
    private readonly ILogger<AzureChatCompletionProvider> _logger;

    public AzureChatCompletionProvider(
        IOptions<AzureOpenAiOptions> options,
        IOptions<AiProviderOptions> providerOptions,
        ILogger<AzureChatCompletionProvider> logger)
    {
        _options = options.Value;
        _providerOptions = providerOptions.Value;
        _logger = logger;

        _client = new OpenAIClient(
            new Uri(_options.Endpoint),
            new AzureKeyCredential(_options.ApiKey));
    }

    public async Task<ChatCompletionResult> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt cannot be empty");

        return await RetryPolicyHelper.ExecuteAsync(
            async () =>
            {
                _logger.LogDebug(
                    "Generating chat completion using Azure OpenAI deployment {Deployment}",
                    _options.ChatDeployment);

                var chatOptions = new ChatCompletionsOptions
                {
                    Temperature = 0.0f, // deterministic answers for RAG
                };

                
                var response =
                    await _client.GetChatCompletionsAsync(
                        new ChatCompletionsOptions(),
                        cancellationToken);

                var choice = response.Value.Choices.FirstOrDefault();

                if (choice?.Message?.Content == null)
                    throw new InvalidOperationException(
                        "Azure OpenAI returned empty chat response");

                return new ChatCompletionResult
                {
                    Answer = choice.Message.Content.Trim(),
                    PromptTokens = response.Value.Usage?.PromptTokens ?? 0,
                    CompletionTokens = response.Value.Usage?.CompletionTokens ?? 0
                };
            },
            _providerOptions.MaxRetries,
            _providerOptions.RetryDelayMs);
    }
}
