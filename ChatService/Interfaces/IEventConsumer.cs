namespace ChatService.Interfaces;

public interface IEventConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}