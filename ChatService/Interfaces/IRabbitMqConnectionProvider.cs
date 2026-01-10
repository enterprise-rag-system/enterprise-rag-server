using RabbitMQ.Client;

namespace ChatService.Interfaces;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnectionAsync();
}