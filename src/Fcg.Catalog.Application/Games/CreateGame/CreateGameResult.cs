namespace Fcg.Catalog.Application.Games.CreateGame;

public sealed record CreateGameResult(
    Guid GameId,
    string Title,
    string Description,
    decimal Price,
    string Category,
    bool Active,
    DateTime CreatedAt);
