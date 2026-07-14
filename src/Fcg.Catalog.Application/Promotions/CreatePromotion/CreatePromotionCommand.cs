namespace Fcg.Catalog.Application.Promotions.CreatePromotion;

public sealed record CreatePromotionCommand(
    Guid GameId,
    string Name,
    string Description,
    decimal DiscountPercentage,
    DateTime StartsAt,
    DateTime EndsAt);
