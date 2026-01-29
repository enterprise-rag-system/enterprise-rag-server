using ChatService.Infrastructure;
using ChatService.Interfaces;
using ChatService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Repositories;

public class ChatRepository: IChatRepository
{
    
    private readonly AppDbContext _db;

    public ChatRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ChatMessage message, CancellationToken ct = default)
    {
        await _db.ChatMessages.AddAsync(message, ct);
        await _db.SaveChangesAsync(ct);

    }

    public async Task<IReadOnlyList<ChatMessage>> GetByProjectAsync(
        Guid projectId,
        int limit,
        CancellationToken ct = default)
    {
        return await _db.ChatMessages
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken ct = default)
    {
        return await _db.ChatMessages
            .Where(x => x.CorrelationId == correlationId)
            .OrderBy(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task SoftDeleteByProjectAsync(
        Guid projectId,
        CancellationToken ct = default)
    {
        await _db.ChatMessages
            .Where(x => x.ProjectId == projectId)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(x => x.IsDeleted, true),
                ct);
    }
}
