using Fcg.Catalog.Application.Common;
using Fcg.Catalog.Application.Events;
using Fcg.Catalog.Domain.Common;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Libraries;

namespace Fcg.Catalog.Application.Purchases.PlaceOrder;

public sealed class PlaceOrderUseCase : IPlaceOrderUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILibraryItemRepository _libraryItemRepository;
    private readonly IEventPublisher _eventPublisher;

    public PlaceOrderUseCase(
        IGameRepository gameRepository,
        ILibraryItemRepository libraryItemRepository,
        IEventPublisher? eventPublisher = null)
    {
        _gameRepository = gameRepository;
        _libraryItemRepository = libraryItemRepository;
        _eventPublisher = eventPublisher ?? new NullEventPublisher();
    }

    public async Task<PlaceOrderResult> ExecuteAsync(
        PlaceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
        {
            throw new DomainValidationException("User id is required.");
        }

        var game = await _gameRepository.GetByIdAsync(command.GameId, cancellationToken);
        if (game is null)
        {
            throw new ResourceNotFoundException("Game was not found.");
        }

        if (!game.Active)
        {
            throw new DomainValidationException("Only active games can be purchased.");
        }

        var alreadyOwned = await _libraryItemRepository.ExistsAsync(
            command.UserId,
            command.GameId,
            cancellationToken);
        if (alreadyOwned)
        {
            throw new DomainValidationException("Game is already in the user's library.");
        }

        var orderId = Guid.NewGuid();
        var placedAt = DateTime.UtcNow;

        await _eventPublisher.PublishAsync(
            new OrderPlacedEvent(
                orderId,
                command.UserId,
                command.GameId,
                game.Price,
                placedAt),
            cancellationToken);

        return new PlaceOrderResult(
            orderId,
            command.UserId,
            command.GameId,
            game.Price,
            "PendingPayment",
            placedAt);
    }
}
