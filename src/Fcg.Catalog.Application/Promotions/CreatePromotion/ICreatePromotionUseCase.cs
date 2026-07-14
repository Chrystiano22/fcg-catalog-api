namespace Fcg.Catalog.Application.Promotions.CreatePromotion;

public interface ICreatePromotionUseCase
{
    Task<CreatePromotionResult> ExecuteAsync(
        CreatePromotionCommand command,
        CancellationToken cancellationToken = default);
}
