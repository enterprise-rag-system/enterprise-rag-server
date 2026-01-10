namespace ChatService.Models.DTOs;

public sealed class ChatHistoryResponse
{
    public Guid ProjectId { get; set; }
    public IReadOnlyList<ChatMessageResponse> Messages { get; set; }
        = Array.Empty<ChatMessageResponse>();
}
