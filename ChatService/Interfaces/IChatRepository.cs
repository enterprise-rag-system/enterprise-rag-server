using ChatService.Models.Entities;

namespace ChatService.Interfaces;

public interface IChatRepository
{
    
        Task AddAsync(ChatMessage message, CancellationToken ct = default);

        Task<IReadOnlyList<ChatMessage>> GetByProjectAsync(
            Guid projectId,
            int limit,
            CancellationToken ct = default);

        Task<IReadOnlyList<ChatMessage>> GetByCorrelationIdAsync(
            string correlationId,
            CancellationToken ct = default);

        Task SoftDeleteByProjectAsync(
            Guid projectId,
            CancellationToken ct = default);
    

}
