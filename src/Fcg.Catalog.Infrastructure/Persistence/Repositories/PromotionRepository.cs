using Fcg.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Infrastructure.Persistence.Repositories;

public sealed class PromotionRepository : IPromotionRepository
{
    private readonly CatalogDbContext _dbContext;

    public PromotionRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default)
    {
        await _dbContext.Promotions.AddAsync(promotion, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Promotion>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Promotions
            .AsNoTracking()
            .OrderBy(promotion => promotion.StartsAt)
            .ToListAsync(cancellationToken);
    }
}
