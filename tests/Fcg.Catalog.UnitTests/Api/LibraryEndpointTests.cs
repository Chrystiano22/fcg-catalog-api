using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Libraries;
using Fcg.Catalog.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Catalog.UnitTests.Api;

public sealed class LibraryEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LibraryEndpointTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBiblioteca_WithAuthenticatedUser_ReturnsOwnedGames()
    {
        var userId = Guid.NewGuid();
        var game = await SeedOwnedGameAsync(userId);
        var userToken = TestJwtTokenFactory.CreateUserToken(userId);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, "/biblioteca");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var response = await _client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LibraryItemHttpResponse[]>(JsonOptions);

        Assert.NotNull(payload);
        Assert.NotEmpty(payload);
        Assert.Contains(payload, item => item.JogoId == game.Id && item.Titulo == game.Title);
    }

    [Fact]
    public async Task PostCompras_WithAuthenticatedUser_ReturnsAccepted()
    {
        var adminToken = TestJwtTokenFactory.CreateAdministratorToken();
        var userToken = TestJwtTokenFactory.CreateUserToken();
        var gameId = await CreateGameAsync(adminToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/compras")
        {
            Content = JsonContent.Create(new
            {
                jogoId = gameId
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PurchaseHttpResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(gameId, payload.JogoId);
        Assert.Equal("PendingPayment", payload.Status);
    }

    [Fact]
    public async Task PostCompras_WithOwnedGame_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var game = await SeedOwnedGameAsync(userId);
        var userToken = TestJwtTokenFactory.CreateUserToken(userId);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/compras")
        {
            Content = JsonContent.Create(new
            {
                jogoId = game.Id
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Game> SeedOwnedGameAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var game = Game.Create(
            $"Architecture Quest {Guid.NewGuid():N}",
            "Educational game about software architecture.",
            79.90m,
            "Education");
        var libraryItem = LibraryItem.Acquire(userId, game.Id);

        await dbContext.Games.AddAsync(game);
        await dbContext.LibraryItems.AddAsync(libraryItem);
        await dbContext.SaveChangesAsync();

        return game;
    }

    private async Task<Guid> CreateGameAsync(string adminToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/jogos")
        {
            Content = JsonContent.Create(new
            {
                titulo = $"Architecture Quest {Guid.NewGuid():N}",
                descricao = "Educational game about software architecture.",
                preco = 79.90m,
                categoria = "Education"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.SendAsync(request);
        var payload = await response.Content.ReadFromJsonAsync<GameHttpResponse>(JsonOptions);

        return payload!.Id;
    }

    private sealed class GameHttpResponse
    {
        public Guid Id { get; init; }
    }

    private sealed class LibraryItemHttpResponse
    {
        public Guid JogoId { get; init; }

        public string Titulo { get; init; } = string.Empty;
    }

    private sealed class PurchaseHttpResponse
    {
        public Guid JogoId { get; init; }

        public string Status { get; init; } = string.Empty;
    }
}
