using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RagWorker.Interfaces.Messaging;

namespace RagWorker.Infrastructure.Messaging;

public sealed class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
{
    private readonly ILogger<RabbitMqMessageBus> _logger;
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IChannel? _channel;
    
    private const int MaxRetryCount = 5;
    private const string RetryHeader = "x-retry-count";


    private string Exchange => _configuration["RabbitMQ:Exchange"]!;

    public RabbitMqMessageBus(
        IConfiguration configuration,
        ILogger<RabbitMqMessageBus> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /* -------------------------------------------------
     * Connection / Channel
     * ------------------------------------------------- */
    private async Task EnsureConnectedAsync()
    {
        if (_connection != null && _channel != null)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Hostname"],
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"],
            ConsumerDispatchConcurrency = 1
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        _logger.LogInformation(
            "RabbitMQ connected. Exchange: {Exchange}",
            Exchange);
    }

    /* -------------------------------------------------
     * Publish
     * ------------------------------------------------- */
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await EnsureConnectedAsync();

        var routingKey = typeof(TEvent).Name;

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(@event));

        var properties = new BasicProperties
        {
            Persistent = true
        };

        await _channel!.BasicPublishAsync(
            exchange: Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published event {EventType} to exchange {Exchange}",
            routingKey,
            Exchange);
    }

    /* -------------------------------------------------
     * Subscribe
     * ------------------------------------------------- */
    public async Task SubscribeAsync<TEvent>(
        string queueName,
        Func<TEvent, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await EnsureConnectedAsync();

        var routingKey = typeof(TEvent).Name;
        
        var retryQueue = $"{queueName}.retry";
        var dlqQueue = $"{queueName}.dlq";


        // DLQ
        await _channel!.QueueDeclareAsync(
            queue: dlqQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Retry Queue (TTL + DLX back to main queue)
        await _channel.QueueDeclareAsync(
            queue: retryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-message-ttl"] = 30000, // 30 seconds
                ["x-dead-letter-exchange"] = Exchange,
                ["x-dead-letter-routing-key"] = routingKey
            });

        // Main Queue → sends failed messages to retry queue
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = "",
                ["x-dead-letter-routing-key"] = retryQueue
            });
        
        // Bind queue to exchange with routing key
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: Exchange,
            routingKey: routingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            var retryCount = 0;

            if (args.BasicProperties.Headers != null &&
                args.BasicProperties.Headers.TryGetValue(RetryHeader, out var value))
            {
                retryCount = Convert.ToInt32(value);
            }
            
            try
            {
                var json =
                    Encoding.UTF8.GetString(args.Body.ToArray());

                var message =
                    JsonSerializer.Deserialize<TEvent>(json);

                if (message == null)
                    throw new InvalidOperationException(
                        "Deserialized message is null");

                await handler(message);

                await _channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing event {EventType} on queue {Queue}",
                    routingKey,
                    queueName);

                if (retryCount >= MaxRetryCount)
                {
                    var props = new BasicProperties
                    {
                        Persistent = true,
                        Headers = new Dictionary<string, object>
                        {
                            [RetryHeader] = retryCount
                        }
                    };
                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: dlqQueue,
                        mandatory: true,
                        basicProperties: props ,
                        body: args.Body);

                    await _channel.BasicAckAsync(args.DeliveryTag, false);

                }
                else
                {
                    var props = new BasicProperties
                    {
                        Persistent = true,
                        Headers = new Dictionary<string, object>
                        {
                            [RetryHeader] = retryCount + 1
                        }
                    };
                    // Send to retry queue with incremented retry count
                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: retryQueue,
                        mandatory: true,
                        basicProperties: props,
                        body: args.Body);

                    await _channel.BasicAckAsync(args.DeliveryTag, false);

                }
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Subscribed to {EventType} on queue {Queue}",
            routingKey,
            queueName);
    }
    
    private IBasicProperties CreateRetryProperties(int retryCount)
    {
        var props = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object>
            {
                [RetryHeader] = retryCount
            }
        };
        return props;
    }


    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();
    }
}
