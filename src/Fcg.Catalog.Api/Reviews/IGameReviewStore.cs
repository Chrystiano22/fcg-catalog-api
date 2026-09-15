namespace Fcg.Catalog.Api.Reviews;

public interface IGameReviewStore
{
    Task<GameReviewDocument> AddAsync(
        Guid gameId,
        Guid userId,
        int rating,
        string comment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GameReviewDocument>> ListByGameAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);
}
