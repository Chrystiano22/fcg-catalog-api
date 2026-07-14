namespace Fcg.Catalog.Application.Events;

public sealed class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        return Task.CompletedTask;
    }
}
