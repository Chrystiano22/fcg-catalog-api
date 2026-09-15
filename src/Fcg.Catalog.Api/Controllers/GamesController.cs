using System.Text.Json;
using Fcg.Catalog.Api.Contracts.Games;
using Fcg.Catalog.Application.Games.CreateGame;
using Fcg.Catalog.Application.Games.DeleteGame;
using Fcg.Catalog.Application.Games.ListGames;
using Fcg.Catalog.Application.Games.UpdateGame;
using Fcg.Catalog.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Fcg.Catalog.Api.Controllers;

[ApiController]
[Route("jogos")]
[Authorize]
public sealed class GamesController : ControllerBase
{
    private const string CatalogGamesCacheKey = "catalog:games:list";
    private static readonly DistributedCacheEntryOptions CatalogGamesCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<GameResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<GameResponse>>> Get(
        [FromServices] IListGamesUseCase useCase,
        [FromServices] IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        var cachedJson = await cache.GetStringAsync(CatalogGamesCacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            var cachedGames = JsonSerializer.Deserialize<GameResponse[]>(cachedJson);
            if (cachedGames is not null)
            {
                return Ok(cachedGames);
            }
        }

        var result = await useCase.ExecuteAsync(cancellationToken);
        var response = result.Select(GameResponse.FromListResult).ToArray();

        await cache.SetStringAsync(
            CatalogGamesCacheKey,
            JsonSerializer.Serialize(response),
            CatalogGamesCacheOptions,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GameResponse>> Post(
        [FromBody] CreateGameRequest request,
        [FromServices] ICreateGameUseCase useCase,
        [FromServices] IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            new CreateGameCommand(
                request.Titulo,
                request.Descricao,
                request.Preco,
                request.Categoria),
            cancellationToken);

        await cache.RemoveAsync(CatalogGamesCacheKey, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, GameResponse.FromCreateResult(result));
    }

    [HttpPut("{gameId:guid}")]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GameResponse>> Put(
        Guid gameId,
        [FromBody] UpdateGameRequest request,
        [FromServices] IUpdateGameUseCase useCase,
        [FromServices] IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            gameId,
            new UpdateGameCommand(
                request.Titulo,
                request.Descricao,
                request.Preco,
                request.Categoria),
            cancellationToken);

        await cache.RemoveAsync(CatalogGamesCacheKey, cancellationToken);

        return Ok(GameResponse.FromUpdateResult(result));
    }

    [HttpDelete("{gameId:guid}")]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(
        Guid gameId,
        [FromServices] IDeleteGameUseCase useCase,
        [FromServices] IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(gameId, cancellationToken);
        await cache.RemoveAsync(CatalogGamesCacheKey, cancellationToken);

        return NoContent();
    }
}
