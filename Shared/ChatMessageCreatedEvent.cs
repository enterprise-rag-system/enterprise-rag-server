namespace Shared;

public sealed class ChatMessageCreatedEvent
{
    // ---- Event metadata ----
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    // ---- Correlation ----
    public string CorrelationId { get; init; } = null!;

    // ---- Scope ----
    public Guid ProjectId { get; init; }

    // ---- Traceability ----
    public Guid ChatMessageId { get; init; }

    // ---- Payload (AI needs this) ----
    public string Content { get; init; } = null!;
}
