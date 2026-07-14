using Fcg.Catalog.Application.Libraries.AcquireGameForUser;
using Fcg.Catalog.Domain.Common;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Libraries;

namespace Fcg.Catalog.UnitTests.Libraries;

public sealed class AcquireGameForUserUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidData_AddsGameToLibrary()
    {
        var userId = Guid.NewGuid();
        var game = Game.Create(
            "Architecture Quest",
            "Educational game.",
            79.90m,
            "Education");

        var libraryRepository = new FakeLibraryRepository();
        var useCase = new AcquireGameForUserUseCase(
            new FakeGameRepository(game),
            libraryRepository);

        var result = await useCase.ExecuteAsync(userId, game.Id);

        Assert.Equal(userId, result.UserId);
        Assert.Equal(game.Id, result.GameId);
        Assert.Single(libraryRepository.AddedItems);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateGame_ThrowsDomainValidationException()
    {
        var game = Game.Create(
            "Architecture Quest",
            "Educational game.",
            79.90m,
            "Education");

        var useCase = new AcquireGameForUserUseCase(
            new FakeGameRepository(game),
            new FakeLibraryRepository { Exists = true });

        var action = () => useCase.ExecuteAsync(Guid.NewGuid(), game.Id);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(action);

        Assert.Equal("Game is already in the user's library.", exception.Message);
    }

    private sealed class FakeGameRepository : IGameRepository
    {
        private readonly Game? _game;

        public FakeGameRepository(Game? game)
        {
            _game = game;
        }

        public Task AddAsync(Game game, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Game?> GetByIdAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_game?.Id == gameId ? _game : null);
        }

        public Task<IReadOnlyCollection<Game>> ListAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Game> result = _game is null ? [] : [_game];
            return Task.FromResult(result);
        }

        public Task UpdateAsync(Game game, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeLibraryRepository : ILibraryItemRepository
    {
        public bool Exists { get; init; }

        public List<LibraryItem> AddedItems { get; } = [];

        public Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exists);
        }

        public Task AddAsync(LibraryItem libraryItem, CancellationToken cancellationToken = default)
        {
            AddedItems.Add(libraryItem);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<LibraryItem>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<LibraryItem>>(AddedItems);
        }
    }
}
