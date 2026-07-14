namespace Fcg.Catalog.Application.Events;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    DateTime PlacedAt);
