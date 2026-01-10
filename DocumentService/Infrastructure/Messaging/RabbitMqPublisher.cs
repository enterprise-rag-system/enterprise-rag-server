using System.Text;
using System.Text.Json;
using DocumentService.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Shared;

namespace DocumentService.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private readonly string _exchange;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;

        _exchange = _configuration.GetValue<string>("RabbitMQ:Exchange")!;

        var factory = new ConnectionFactory
        {
            HostName = _configuration.GetValue<string>("RabbitMQ:Hostname"),
            UserName = _configuration.GetValue<string>("RabbitMQ:Username"),
            Password = _configuration.GetValue<string>("RabbitMQ:Password"),
            ConsumerDispatchConcurrency = 1
        };

        
        _connection = factory.CreateConnectionAsync()
            .GetAwaiter()
            .GetResult();

        _channel = _connection.CreateChannelAsync()
            .GetAwaiter()
            .GetResult();

        
        _channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false)
            .GetAwaiter()
            .GetResult();
    }

    public async Task PublishMessage(DocumentUploadedEvent evt)
    {
        var routingKey = nameof(DocumentUploadedEvent);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(evt));

        var props = new BasicProperties
        {
            Persistent = true
        };

        await _channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body);
        
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();
    }
}
