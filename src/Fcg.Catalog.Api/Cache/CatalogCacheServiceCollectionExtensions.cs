using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace Fcg.Catalog.Api.Cache;

public static class CatalogCacheServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration["Redis:ConnectionString"];

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddDistributedMemoryCache();

            return services;
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = configuration["Redis:InstanceName"] ?? "fcg-catalog:";
        });

        return services;
    }
}
