using RagWorker.Interfaces.AI;

namespace RagWorker.Providers.Grok;

public class GrokOptions: IAiProviderConnectionOptions, IChatModelOptions
{
    public string BaseUrl { get; set; } = default!;
    public string? ApiKey { get; set; }
    public string ChatModel { get; set; } = "grok-2-latest";

}