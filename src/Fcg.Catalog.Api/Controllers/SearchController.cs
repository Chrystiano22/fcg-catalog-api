using Fcg.Catalog.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Catalog.Api.Controllers;

[ApiController]
[Authorize]
[Route("jogos/busca")]
public sealed class SearchController(IGameSearch search) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GameSearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<GameSearchResult> Get([FromQuery] GameSearchQuery query, CancellationToken cancellationToken) =>
        search.SearchAsync(query, cancellationToken);
}
