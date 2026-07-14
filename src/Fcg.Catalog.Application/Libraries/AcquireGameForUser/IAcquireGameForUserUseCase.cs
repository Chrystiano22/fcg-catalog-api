namespace Fcg.Catalog.Application.Libraries.AcquireGameForUser;

public interface IAcquireGameForUserUseCase
{
    Task<AcquireGameForUserResult> ExecuteAsync(
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken = default);
}
