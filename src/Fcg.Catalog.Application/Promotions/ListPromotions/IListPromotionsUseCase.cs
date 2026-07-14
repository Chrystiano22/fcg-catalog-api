namespace Fcg.Catalog.Application.Promotions.ListPromotions;

public interface IListPromotionsUseCase
{
    Task<IReadOnlyCollection<ListPromotionsResult>> ExecuteAsync(
        CancellationToken cancellationToken = default);
}
