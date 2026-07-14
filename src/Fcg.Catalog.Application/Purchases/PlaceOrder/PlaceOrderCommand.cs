namespace Fcg.Catalog.Application.Purchases.PlaceOrder;

public sealed record PlaceOrderCommand(Guid UserId, Guid GameId);
