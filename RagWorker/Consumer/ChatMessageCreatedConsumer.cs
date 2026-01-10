using RagWorker.Interfaces.Messaging;
using RagWorker.Interfaces.Rag;
using RagWorker.Models.Rag;
using Shared;

namespace RagWorker.Consumer;

public sealed class ChatMessageCreatedConsumer : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatMessageCreatedConsumer> _logger;

    public ChatMessageCreatedConsumer(
        IMessageBus messageBus,
        IServiceProvider serviceProvider,
        ILogger<ChatMessageCreatedConsumer> logger)
    {
        _messageBus = messageBus;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ChatMessageCreatedWorker started");

        await _messageBus.SubscribeAsync<ChatMessageCreatedEvent>("ers.qu.rag.worker",
            async evt =>
            {
                using var scope =
                    _logger.BeginScope(
                        "CorrelationId:{CorrelationId}",
                        evt.CorrelationId);

                try
                {
                    _logger.LogInformation(
                        "Processing chat message {ChatMessageId} for project {ProjectId}",
                        evt.ChatMessageId,
                        evt.ProjectId);

                    using var ragscope = _serviceProvider.CreateScope();
                    var ragProcessor =
                        ragscope.ServiceProvider.GetRequiredService<IRagQueryProcessor>();

                    var result =
                        await ragProcessor.ProcessAsync(
                            new RagQuery
                            {
                                ProjectId = evt.ProjectId,
                                Question = evt.Content
                            },
                            stoppingToken);

                    var responseEvent =
                        new AIResponseGeneratedEvent
                        {
                            EventId = Guid.NewGuid().ToString(),
                            CorrelationId = evt.CorrelationId,
                            ProjectId = evt.ProjectId,
                            Answer = result.Answer,
                            SourceChunkIds = result.SourceChunkIds,
                            TokenUsage = result.TotalTokens,
                            OccurredAtUtc = DateTime.UtcNow
                        };

                    await _messageBus.PublishAsync(
                        responseEvent,
                        stoppingToken);

                    _logger.LogInformation(
                        "AI response published for correlation {CorrelationId}",
                        evt.CorrelationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process chat message {ChatMessageId}",
                        evt.ChatMessageId);
                    
                    throw;
                }
            },
            stoppingToken);
    }
}
