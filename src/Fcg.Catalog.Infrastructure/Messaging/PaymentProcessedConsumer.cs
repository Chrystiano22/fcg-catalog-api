using Fcg.Catalog.Application.Events;
using Fcg.Catalog.Application.Purchases.ProcessPayment;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Fcg.Catalog.Infrastructure.Messaging;

public sealed class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IProcessPaymentUseCase _useCase;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(
        IProcessPaymentUseCase useCase,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var result = await _useCase.ExecuteAsync(context.Message, context.CancellationToken);

        _logger.LogInformation(
            "PaymentProcessedEvent consumed for order {OrderId}. Added to library: {AddedToLibrary}.",
            result.OrderId,
            result.AddedToLibrary);
    }
}
