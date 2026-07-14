using Fcg.Catalog.Application.Events;
using Fcg.Catalog.Application.Libraries.AcquireGameForUser;
using Fcg.Catalog.Application.Purchases.ProcessPayment;

namespace Fcg.Catalog.UnitTests.Purchases;

public sealed class ProcessPaymentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WithApprovedPayment_AddsGameToLibrary()
    {
        var acquireUseCase = new FakeAcquireGameForUserUseCase();
        var useCase = new ProcessPaymentUseCase(acquireUseCase);
        var paymentEvent = CreateEvent("Approved");

        var result = await useCase.ExecuteAsync(paymentEvent);

        Assert.True(result.AddedToLibrary);
        Assert.Single(acquireUseCase.Acquisitions);
        Assert.Equal(paymentEvent.UserId, acquireUseCase.Acquisitions.Single().UserId);
        Assert.Equal(paymentEvent.GameId, acquireUseCase.Acquisitions.Single().GameId);
    }

    [Fact]
    public async Task ExecuteAsync_WithRejectedPayment_DoesNotAddGameToLibrary()
    {
        var acquireUseCase = new FakeAcquireGameForUserUseCase();
        var useCase = new ProcessPaymentUseCase(acquireUseCase);
        var paymentEvent = CreateEvent("Rejected");

        var result = await useCase.ExecuteAsync(paymentEvent);

        Assert.False(result.AddedToLibrary);
        Assert.Empty(acquireUseCase.Acquisitions);
    }

    private static PaymentProcessedEvent CreateEvent(string status)
    {
        return new PaymentProcessedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            79.90m,
            status,
            DateTime.UtcNow);
    }

    private sealed class FakeAcquireGameForUserUseCase : IAcquireGameForUserUseCase
    {
        public List<(Guid UserId, Guid GameId)> Acquisitions { get; } = [];

        public Task<AcquireGameForUserResult> ExecuteAsync(
            Guid userId,
            Guid gameId,
            CancellationToken cancellationToken = default)
        {
            Acquisitions.Add((userId, gameId));

            return Task.FromResult(new AcquireGameForUserResult(
                Guid.NewGuid(),
                userId,
                gameId,
                "Architecture Quest",
                "Educational game.",
                79.90m,
                "Education",
                DateTime.UtcNow));
        }
    }
}
