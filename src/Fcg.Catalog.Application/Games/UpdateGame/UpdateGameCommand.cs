namespace Fcg.Catalog.Application.Games.UpdateGame;

public sealed record UpdateGameCommand(
    string Title,
    string Description,
    decimal Price,
    string Category);
