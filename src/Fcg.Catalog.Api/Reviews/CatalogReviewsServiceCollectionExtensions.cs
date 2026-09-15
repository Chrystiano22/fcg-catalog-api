using Fcg.Catalog.Api.Configuration;
using MongoDB.Driver;

namespace Fcg.Catalog.Api.Reviews;

public static class CatalogReviewsServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogReviews(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(MongoCatalogOptions.SectionName)
            .Get<MongoCatalogOptions>() ?? new MongoCatalogOptions();

        if (!options.Enabled)
        {
            services.AddSingleton<IGameReviewStore, DisabledGameReviewStore>();

            return services;
        }

        services.AddSingleton<IMongoClient>(_ => new MongoClient(options.ConnectionString));
        services.AddSingleton(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(options.DatabaseName);

            return database.GetCollection<GameReviewDocument>(options.ReviewsCollectionName);
        });
        services.AddSingleton<IGameReviewStore, MongoGameReviewStore>();

        return services;
    }
}
