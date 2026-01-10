using ChatService.Interfaces;
using RabbitMQ.Client;

namespace ChatService.Infrastructure.Messaging;

public class RabbitMqConnectionProvider: IRabbitMqConnectionProvider, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqConnectionProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync();
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Hostname"],
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"],
                ConsumerDispatchConcurrency = 2
            };

            _connection = await factory.CreateConnectionAsync();
            return _connection;
        }
        finally
        {
            _lock.Release();
        }    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null && _connection.IsOpen)
            await _connection.CloseAsync();
    }
}