using System.Text;
using System.Text.Json;
using ChatService.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;

namespace ChatService.Messaging.Consumer;

public sealed class AIResponseGeneratedConsumer : BackgroundService
{
    private readonly ILogger<AIResponseGeneratedConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly IConfiguration _configuration;

    private IChannel? _channel;

    private const string QueueName = "ers.qu.chat.service";
    private const string RoutingKey = nameof(AIResponseGeneratedEvent);

    private readonly string _exchange;

    public AIResponseGeneratedConsumer(
        ILogger<AIResponseGeneratedConsumer> logger,
        IServiceProvider serviceProvider,
        IRabbitMqConnectionProvider connectionProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connectionProvider = connectionProvider;
        _configuration = configuration;

        _exchange = _configuration.GetValue<string>("RabbitMQ:Exchange")!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMqAsync(stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel!.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "AIResponseGeneratedConsumer started. Queue={Queue}, RoutingKey={RoutingKey}",
            QueueName,
            RoutingKey);
    }

    private async Task InitializeRabbitMqAsync(CancellationToken ct)
    {
        var connection = await _connectionProvider.GetConnectionAsync();

        _channel = await connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: _exchange,
            routingKey: RoutingKey,
            cancellationToken: ct);
    }

    private async Task OnMessageReceivedAsync(
        object sender,
        BasicDeliverEventArgs args)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());

            var evt = JsonSerializer.Deserialize<AIResponseGeneratedEvent>(json)
                      ?? throw new InvalidOperationException("Invalid event payload");

            using var scope = _serviceProvider.CreateScope();
            var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

            await chatService.AddAIResponseAsync(
                evt.ProjectId,
                evt.Answer,
                evt.CorrelationId,
                evt.TokenUsage,
                evt.SourceChunkIds);

            await _channel!.BasicAckAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process AIResponseGeneratedEvent");

            
            await _channel!.BasicNackAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                requeue: true);
        }
    }
}
