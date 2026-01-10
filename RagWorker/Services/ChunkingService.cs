using RagWorker.Interfaces.Ingestion;

namespace RagService.Services;

public class ChunkingService: IChunkingService
{
    public IReadOnlyList<(string Text, int TokenCount)> Chunk(
        string text,
        int size = 800,
        int overlap = 100)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<(string, int)>();

        var chunks = new List<(string, int)>();

        for (int i = 0; i < text.Length; i += size - overlap)
        {
            var length = Math.Min(size, text.Length - i);
            var chunk = text.Substring(i, length).Trim();

            if (string.IsNullOrWhiteSpace(chunk))
                continue;

            // Approx token count (good enough for now)
            var tokenCount = ApproximateTokenCount(chunk);

            chunks.Add((chunk, tokenCount));
        }

        return chunks;
    }

    private static int ApproximateTokenCount(string text)
    {
        // Very close approximation for English
        // Later replace with tokenizer if needed
        return text.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

}