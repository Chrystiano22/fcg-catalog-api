using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.Catalog.Api.Reviews;

public sealed class GameReviewDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    public Guid GameId { get; init; }

    public Guid UserId { get; init; }

    public int Rating { get; init; }

    public string Comment { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}
