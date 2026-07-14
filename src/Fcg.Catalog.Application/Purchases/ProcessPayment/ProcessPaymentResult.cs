namespace Fcg.Catalog.Application.Purchases.ProcessPayment;

public sealed record ProcessPaymentResult(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    string Status,
    bool AddedToLibrary);
