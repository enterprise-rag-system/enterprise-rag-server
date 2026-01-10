namespace RagWorker.Interfaces.Messaging;

public interface IMessageBus
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class;

    Task SubscribeAsync<TEvent>(
        string queueName,
        Func<TEvent, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : class;
}