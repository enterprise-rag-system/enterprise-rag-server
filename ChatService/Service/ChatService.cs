using System.Text.Json;
using ChatService.Infrastructure;
using ChatService.Interfaces;
using ChatService.Models.DTOs;
using ChatService.Models.Entities;
using Shared;

namespace ChatService.Service;

public sealed class ChatService : IChatService
{
    private readonly IChatRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ChatService> _logger;


    public ChatService(
        IChatRepository repository,
        IEventPublisher eventPublisher,
        ILogger<ChatService> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<AddChatMessageResponse> AddUserMessageAsync(Guid projectId, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty", nameof(content));
        }

        var correlationId = $"chat-{Guid.NewGuid():N}";

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ProjectId"] = projectId
        });

        _logger.LogInformation(
            "User chat message received for project");

        try
        {
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Sender = MessageSender.USER,
                Content = content,
                CorrelationId = correlationId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.AddAsync(message, ct);

            _logger.LogInformation(
                "User chat message persisted with MessageId {MessageId}",
                message.Id);

            var evt = new ChatMessageCreatedEvent
            {
                EventId = Guid.NewGuid().ToString(),
                OccurredAtUtc =  DateTime.UtcNow,
                ProjectId = projectId,
                ChatMessageId = message.Id,
                Content = content,
                CorrelationId = correlationId
            };

            await _eventPublisher.PublishAsync(evt, ct);
            
            
            _logger.LogInformation(
                "ChatMessageCreatedEvent published successfully");
            return new AddChatMessageResponse{ CorrelationId =  correlationId };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process user chat message");

            throw;
        }

    }


    public async Task AddAIResponseAsync(Guid projectId, string answer, string correlationId, int tokenUsage,
        IReadOnlyList<Guid> sourceChunkIds, CancellationToken ct = default)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ProjectId"] = projectId
        });

        _logger.LogInformation(
            "AI response received for project");

        try
        {
            var aiMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Sender = MessageSender.AI,
                Content = answer,
                CorrelationId = correlationId,
                TokenUsage = tokenUsage,
                SourceChunkIdsJson = JsonSerializer.Serialize(sourceChunkIds),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.AddAsync(aiMessage, ct);

            _logger.LogInformation(
                "AI chat message persisted with TokenUsage {TokenUsage}",
                tokenUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to persist AI response");

            throw;
        }
    }

    public async Task<ChatHistoryResponse> GetChatHistoryAsync(Guid projectId, int limit, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Fetching chat history for ProjectId {ProjectId}",
            projectId);

        var messages = await _repository
            .GetByProjectAsync(projectId, limit, ct);

        return new ChatHistoryResponse
        {
            ProjectId = projectId,
            Messages = messages.Select(x => new ChatMessageResponse
            {
                Id = x.Id.ToString(),
                Sender = x.Sender.ToString(),
                Message = x.Content,
                Timestamp = x.CreatedAt,
                CorrelationId = x.CorrelationId
            }).ToList()
        };
    }
}
