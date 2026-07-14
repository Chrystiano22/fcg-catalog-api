namespace Fcg.Catalog.Application.Games.CreateGame;

public interface ICreateGameUseCase
{
    Task<CreateGameResult> ExecuteAsync(
        CreateGameCommand command,
        CancellationToken cancellationToken = default);
}
