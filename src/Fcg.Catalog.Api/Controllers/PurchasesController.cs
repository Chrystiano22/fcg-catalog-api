using System.Security.Claims;
using Fcg.Catalog.Api.Contracts.Purchases;
using Fcg.Catalog.Application.Purchases.PlaceOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Catalog.Api.Controllers;

[ApiController]
[Route("compras")]
[Authorize]
public sealed class PurchasesController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PurchaseResponse>> Post(
        [FromBody] CreatePurchaseRequest request,
        [FromServices] IPlaceOrderUseCase useCase,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var result = await useCase.ExecuteAsync(
            new PlaceOrderCommand(parsedUserId, request.JogoId),
            cancellationToken);

        return Accepted(PurchaseResponse.FromResult(result));
    }
}
