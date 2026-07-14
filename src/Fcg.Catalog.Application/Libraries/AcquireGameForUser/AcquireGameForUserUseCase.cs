using Fcg.Catalog.Application.Common;
using Fcg.Catalog.Domain.Common;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Libraries;

namespace Fcg.Catalog.Application.Libraries.AcquireGameForUser;

public sealed class AcquireGameForUserUseCase : IAcquireGameForUserUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILibraryItemRepository _libraryItemRepository;

    public AcquireGameForUserUseCase(
        IGameRepository gameRepository,
        ILibraryItemRepository libraryItemRepository)
    {
        _gameRepository = gameRepository;
        _libraryItemRepository = libraryItemRepository;
    }

    public async Task<AcquireGameForUserResult> ExecuteAsync(
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game is null)
        {
            throw new ResourceNotFoundException("Game was not found.");
        }

        if (!game.Active)
        {
            throw new DomainValidationException("Only active games can be added to the library.");
        }

        var alreadyOwned = await _libraryItemRepository.ExistsAsync(userId, gameId, cancellationToken);
        if (alreadyOwned)
        {
            throw new DomainValidationException("Game is already in the user's library.");
        }

        var libraryItem = LibraryItem.Acquire(userId, gameId);

        await _libraryItemRepository.AddAsync(libraryItem, cancellationToken);

        return new AcquireGameForUserResult(
            libraryItem.Id,
            userId,
            gameId,
            game.Title,
            game.Description,
            game.Price,
            game.Category,
            libraryItem.AcquiredAt);
    }
}
