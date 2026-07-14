using Fcg.Catalog.Application.Purchases.PlaceOrder;

namespace Fcg.Catalog.Api.Contracts.Purchases;

public sealed class PurchaseResponse
{
    public Guid PedidoId { get; init; }

    public Guid UsuarioId { get; init; }

    public Guid JogoId { get; init; }

    public decimal Preco { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime CriadoEm { get; init; }

    public static PurchaseResponse FromResult(PlaceOrderResult result)
    {
        return new PurchaseResponse
        {
            PedidoId = result.OrderId,
            UsuarioId = result.UserId,
            JogoId = result.GameId,
            Preco = result.Price,
            Status = result.Status,
            CriadoEm = result.PlacedAt
        };
    }
}
