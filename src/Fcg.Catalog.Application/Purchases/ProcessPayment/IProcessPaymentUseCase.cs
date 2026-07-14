using Fcg.Catalog.Application.Events;

namespace Fcg.Catalog.Application.Purchases.ProcessPayment;

public interface IProcessPaymentUseCase
{
    Task<ProcessPaymentResult> ExecuteAsync(
        PaymentProcessedEvent paymentProcessedEvent,
        CancellationToken cancellationToken = default);
}
