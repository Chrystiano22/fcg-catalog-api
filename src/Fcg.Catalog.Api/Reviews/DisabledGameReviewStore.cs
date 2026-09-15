namespace Fcg.Catalog.Api.Reviews;

public sealed class DisabledGameReviewStore : IGameReviewStore
{
    public Task<GameReviewDocument> AddAsync(
        Guid gameId,
        Guid userId,
        int rating,
        string comment,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("MongoDB review storage is disabled.");
    }

    public Task<IReadOnlyCollection<GameReviewDocument>> ListByGameAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<GameReviewDocument>>(Array.Empty<GameReviewDocument>());
    }
}
