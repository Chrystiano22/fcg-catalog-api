using System.Security.Claims;
using Fcg.Catalog.Api.Contracts.Reviews;
using Fcg.Catalog.Api.Reviews;
using Fcg.Catalog.Application.Common;
using Fcg.Catalog.Domain.Common;
using Fcg.Catalog.Domain.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Catalog.Api.Controllers;

[ApiController]
[Authorize]
[Route("jogos/{gameId:guid}/avaliacoes")]
public sealed class ReviewsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<ReviewResponse>>> Get(
        Guid gameId,
        [FromServices] IGameReviewStore reviewStore,
        CancellationToken cancellationToken)
    {
        var reviews = await reviewStore.ListByGameAsync(gameId, cancellationToken);

        return Ok(reviews.Select(ReviewResponse.FromDocument).ToArray());
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReviewResponse>> Post(
        Guid gameId,
        [FromBody] CreateReviewRequest request,
        [FromServices] IGameRepository gameRepository,
        [FromServices] IGameReviewStore reviewStore,
        CancellationToken cancellationToken)
    {
        if (request.Nota is < 1 or > 5)
        {
            throw new DomainValidationException("Rating must be between 1 and 5.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var game = await gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game is null || !game.Active)
        {
            throw new ResourceNotFoundException("Game was not found.");
        }

        var review = await reviewStore.AddAsync(
            gameId,
            parsedUserId,
            request.Nota,
            request.Comentario,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ReviewResponse.FromDocument(review));
    }
}
