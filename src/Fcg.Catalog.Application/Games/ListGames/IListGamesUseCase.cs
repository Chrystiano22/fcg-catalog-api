namespace Fcg.Catalog.Application.Games.ListGames;

public interface IListGamesUseCase
{
    Task<IReadOnlyCollection<ListGamesResult>> ExecuteAsync(
        CancellationToken cancellationToken = default);
}
