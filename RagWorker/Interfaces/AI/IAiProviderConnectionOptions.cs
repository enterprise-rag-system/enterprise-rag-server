namespace RagWorker.Interfaces.AI;

public interface IAiProviderConnectionOptions
{
    string BaseUrl { get; }
    string? ApiKey { get; }
}