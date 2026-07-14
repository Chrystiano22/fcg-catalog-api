using Fcg.Catalog.Application.Events;
using Fcg.Catalog.Application.Libraries.AcquireGameForUser;

namespace Fcg.Catalog.Application.Purchases.ProcessPayment;

public sealed class ProcessPaymentUseCase : IProcessPaymentUseCase
{
    private readonly IAcquireGameForUserUseCase _acquireGameForUserUseCase;

    public ProcessPaymentUseCase(IAcquireGameForUserUseCase acquireGameForUserUseCase)
    {
        _acquireGameForUserUseCase = acquireGameForUserUseCase;
    }

    public async Task<ProcessPaymentResult> ExecuteAsync(
        PaymentProcessedEvent paymentProcessedEvent,
        CancellationToken cancellationToken = default)
    {
        var approved = string.Equals(
            paymentProcessedEvent.Status,
            "Approved",
            StringComparison.OrdinalIgnoreCase);

        if (approved)
        {
            await _acquireGameForUserUseCase.ExecuteAsync(
                paymentProcessedEvent.UserId,
                paymentProcessedEvent.GameId,
                cancellationToken);
        }

        return new ProcessPaymentResult(
            paymentProcessedEvent.OrderId,
            paymentProcessedEvent.UserId,
            paymentProcessedEvent.GameId,
            paymentProcessedEvent.Status,
            approved);
    }
}
