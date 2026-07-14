namespace Fcg.Catalog.Application.Games.DeleteGame;

public interface IDeleteGameUseCase
{
    Task ExecuteAsync(Guid gameId, CancellationToken cancellationToken = default);
}
