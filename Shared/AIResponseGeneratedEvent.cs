namespace Shared;

public class AIResponseGeneratedEvent
{
    // -------- Event metadata --------
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    // -------- Correlation --------
    public string CorrelationId { get; init; } = null!;

    // -------- Scope --------
    public Guid ProjectId { get; init; }

    // -------- Traceability --------
    public Guid SourceChatMessageId { get; init; }

    // -------- AI Output --------
    public string Answer { get; init; } = null!;
    public int TokenUsage { get; init; }

    // -------- Explainability --------
    public IReadOnlyList<Guid> SourceChunkIds { get; init; }
        = Array.Empty<Guid>();

}