namespace RagWorker.Interfaces.Ingestion;

public interface IChunkingService
{
    IReadOnlyList<(string Text, int TokenCount)> Chunk(
        string text,
        int size = 800,
        int overlap = 100);

}