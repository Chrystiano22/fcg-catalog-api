using Fcg.Catalog.Application.Events;
using Microsoft.Extensions.Logging;

namespace Fcg.Catalog.Infrastructure.Events;

public sealed class LoggingEventPublisher : IEventPublisher
{
    private readonly ILogger<LoggingEventPublisher> _logger;

    public LoggingEventPublisher(ILogger<LoggingEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        _logger.LogInformation(
            "Event {EventType} published: {@Event}",
            typeof(TEvent).Name,
            @event);

        return Task.CompletedTask;
    }
}
