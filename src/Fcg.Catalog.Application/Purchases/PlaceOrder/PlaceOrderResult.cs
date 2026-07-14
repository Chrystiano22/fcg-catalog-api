namespace Fcg.Catalog.Application.Purchases.PlaceOrder;

public sealed record PlaceOrderResult(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    string Status,
    DateTime PlacedAt);
