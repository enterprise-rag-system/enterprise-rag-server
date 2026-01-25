using Microsoft.Extensions.Options;
using RagWorker.Interfaces.AI;
using RagWorker.Interfaces.Factory;
using RagWorker.Providers.Azure;
using RagWorker.Providers.Common;
using RagWorker.Providers.Gemini;
using RagWorker.Providers.Grok;
using RagWorker.Providers.Ollama;

namespace RagWorker.Providers.Factory;

public class AiProviderFactory:IChatCompletionProviderFactory, IEmbeddingProviderFactory
{
    private readonly IServiceProvider _sp;
    private readonly AiProviderOptions _options;

    public AiProviderFactory(
        IServiceProvider sp,
        IOptions<AiProviderOptions> options)
    {
        _sp = sp;
        _options = options.Value;
    }

    public IChatCompletionProvider Create()
        => _options.ChatProvider switch
        {
            ProviderConstants.Gemini =>
                _sp.GetRequiredService<GeminiChatCompletionProvider>(),

            ProviderConstants.Grok =>
                _sp.GetRequiredService<GrokChatCompletionProvider>(),

            ProviderConstants.Ollama =>
                _sp.GetRequiredService<OllamaChatCompletionProvider>(),

            ProviderConstants.AzureOpenAi =>
                _sp.GetRequiredService<AzureChatCompletionProvider>(),

            _ => throw new NotSupportedException(
                $"Chat provider '{_options.ChatProvider}' not supported")
        };

    IEmbeddingProvider IEmbeddingProviderFactory.Create()
        => _options.EmbeddingProvider switch
        {
            ProviderConstants.Gemini =>
                _sp.GetRequiredService<GeminiEmbeddingProvider>(),

            ProviderConstants.Ollama =>
                _sp.GetRequiredService<OllamaEmbeddingProvider>(),

            ProviderConstants.AzureOpenAi =>
                _sp.GetRequiredService<AzureEmbeddingProvider>(),

            _ => throw new NotSupportedException(
                $"Embedding provider '{_options.EmbeddingProvider}' not supported")
        };

}