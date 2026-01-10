using Microsoft.Extensions.Options;
using RagWorker.Interfaces.AI;
using RagWorker.Helpers;
using RagWorker.Providers.Common;
using Azure;
using Azure.AI.OpenAI;

namespace RagWorker.Providers.Azure;

public sealed class AzureEmbeddingProvider : IEmbeddingProvider
{
    private readonly OpenAIClient _client;
    private readonly AzureOpenAiOptions _options;
    private readonly AiProviderOptions _providerOptions;
    private readonly ILogger<AzureEmbeddingProvider> _logger;

    public AzureEmbeddingProvider(
        IOptions<AzureOpenAiOptions> options,
        IOptions<AiProviderOptions> providerOptions,
        ILogger<AzureEmbeddingProvider> logger)
    {
        _options = options.Value;
        _providerOptions = providerOptions.Value;
        _logger = logger;

        _client = new OpenAIClient(
            new Uri(_options.Endpoint),
            new AzureKeyCredential(_options.ApiKey));
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input text cannot be empty");

        return await RetryPolicyHelper.ExecuteAsync(
            async () =>
            {
                _logger.LogDebug(
                    "Generating embedding using Azure OpenAI deployment {Deployment}",
                    _options.EmbeddingDeployment);
                var response =
                    await _client.GetEmbeddingsAsync(
                        new EmbeddingsOptions(),
                        cancellationToken: cancellationToken);

                var embedding = response.Value.Data.FirstOrDefault().Embedding.ToArray();
                
                if (embedding == null || embedding.Length == 0)
                    throw new InvalidOperationException(
                        "Azure OpenAI returned empty embedding");

                _logger.LogDebug(
                    "Embedding generated with {Dimension} dimensions",
                    embedding.Length);

                
                return embedding;
            },
            _providerOptions.MaxRetries,
            _providerOptions.RetryDelayMs);
    }
}
