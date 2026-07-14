namespace Fcg.Catalog.Application.Games.UpdateGame;

public interface IUpdateGameUseCase
{
    Task<UpdateGameResult> ExecuteAsync(
        Guid gameId,
        UpdateGameCommand command,
        CancellationToken cancellationToken = default);
}
