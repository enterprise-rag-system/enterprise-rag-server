using ChatService.Models.DTOs;

namespace ChatService.Interfaces;

public interface IChatService
{
    Task<AddChatMessageResponse> AddUserMessageAsync(
        Guid projectId,
        string content,
        CancellationToken ct = default);

    Task AddAIResponseAsync(
        Guid projectId,
        string answer,
        string correlationId,
        int tokenUsage,
        IReadOnlyList<Guid> sourceChunkIds,
        CancellationToken ct = default);

    Task<ChatHistoryResponse> GetChatHistoryAsync(
        Guid projectId,
        int limit,
        CancellationToken ct = default);
}
