namespace ChatService.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken ct = default);
}