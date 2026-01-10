using RabbitMQ.Client;
using RagWorker.Interfaces.Ingestion;
using RagWorker.Interfaces.Messaging;
using Shared;

namespace RagWorker.Consumer;

public sealed class DocumentUploadedConsumer : BackgroundService
{
    private readonly ILogger<DocumentUploadedConsumer> _logger;
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;

    private IChannel? _channel;

    private const string QueueName = "ers.qu.doc.upload";
    private readonly IServiceProvider _serviceProvider;
    

    public DocumentUploadedConsumer(
        ILogger<DocumentUploadedConsumer> logger,
        IMessageBus messageBus,
        IServiceProvider serviceProvider
        )
    {
        _logger = logger;
        _messageBus = messageBus;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "DocumentUploadEventWorker started");
        await _messageBus.SubscribeAsync<DocumentUploadedEvent>(
            queueName: "ers.qu.doc.upload",
            async evt =>
            {
                _logger.LogInformation(
                    "DocumentUploadedEvent received. DocumentId={DocumentId}",
                    evt.DocumentId);

                using var scope = _serviceProvider.CreateScope();
                var ingestionService =
                    scope.ServiceProvider.GetRequiredService<IDocumentIngestionService>();

                await ingestionService.IngestAsync(evt, stoppingToken);
            },
            stoppingToken);

        
        _logger.LogInformation(
            "DocumentUploadedWorker listening on queue {Queue}",
            QueueName);
    }
}
