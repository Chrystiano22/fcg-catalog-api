namespace Fcg.Catalog.Application.Purchases.PlaceOrder;

public interface IPlaceOrderUseCase
{
    Task<PlaceOrderResult> ExecuteAsync(
        PlaceOrderCommand command,
        CancellationToken cancellationToken = default);
}
