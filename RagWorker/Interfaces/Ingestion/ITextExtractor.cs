namespace RagWorker.Interfaces.Ingestion;

public interface ITextExtractor
{
    Task<string> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken);
}