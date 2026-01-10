namespace ChatService.Models.DTOs;

public sealed class InternalAIResponseDto
{
    public Guid ProjectId { get; set; }
    public string Content { get; set; } = null!;
    public string CorrelationId { get; set; } = null!;
    public int TokenUsage { get; set; }
    public IReadOnlyList<string> SourceChunkIds { get; set; }
        = Array.Empty<string>();
}
