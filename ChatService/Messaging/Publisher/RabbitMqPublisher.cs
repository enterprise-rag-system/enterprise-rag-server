using System.Text;
using System.Text.Json;
using ChatService.Interfaces;
using RabbitMQ.Client;

namespace ChatService.Messaging.Publisher;

public sealed class RabbitMqPublisher : IEventPublisher
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly string _exchange;
    private IChannel? _channel;

    public RabbitMqPublisher(
        IRabbitMqConnectionProvider connectionProvider,
        IConfiguration configuration)
    {
        _connectionProvider = connectionProvider;
        _exchange = configuration["RabbitMQ:Exchange"]!;
    }

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel is { IsOpen: true })
            return _channel;

        var connection = await _connectionProvider.GetConnectionAsync();
        _channel = await connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        return _channel;
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken ct = default)
    {
        var channel = await GetChannelAsync();

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(@event));

        var props = new BasicProperties { Persistent = true };

        await channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: typeof(TEvent).Name,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }
}