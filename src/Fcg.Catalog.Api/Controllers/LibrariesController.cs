using System.Security.Claims;
using Fcg.Catalog.Api.Contracts.Libraries;
using Fcg.Catalog.Application.Libraries.AcquireGameForUser;
using Fcg.Catalog.Application.Libraries.GetUserLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Catalog.Api.Controllers;

[ApiController]
public sealed class LibrariesController : ControllerBase
{
    [Authorize]
    [HttpGet("biblioteca")]
    [ProducesResponseType(typeof(IReadOnlyCollection<LibraryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<LibraryItemResponse>>> Get(
        [FromServices] IGetUserLibraryUseCase useCase,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var result = await useCase.ExecuteAsync(parsedUserId, cancellationToken);

        return Ok(result.Select(LibraryItemResponse.FromResult).ToArray());
    }

}
