using MongoDB.Driver;

namespace Fcg.Catalog.Api.Reviews;

public sealed class MongoGameReviewStore : IGameReviewStore
{
    private readonly IMongoCollection<GameReviewDocument> _reviews;

    public MongoGameReviewStore(IMongoCollection<GameReviewDocument> reviews)
    {
        _reviews = reviews;
    }

    public async Task<GameReviewDocument> AddAsync(
        Guid gameId,
        Guid userId,
        int rating,
        string comment,
        CancellationToken cancellationToken = default)
    {
        var document = new GameReviewDocument
        {
            GameId = gameId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        await _reviews.InsertOneAsync(document, cancellationToken: cancellationToken);

        return document;
    }

    public async Task<IReadOnlyCollection<GameReviewDocument>> ListByGameAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var documents = await _reviews
            .Find(review => review.GameId == gameId)
            .SortByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);

        return documents;
    }
}
