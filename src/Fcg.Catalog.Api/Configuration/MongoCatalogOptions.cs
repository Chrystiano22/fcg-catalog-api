namespace Fcg.Catalog.Api.Configuration;

public sealed class MongoCatalogOptions
{
    public const string SectionName = "Mongo";

    public bool Enabled { get; init; }

    public string ConnectionString { get; init; } = "mongodb://localhost:27017";

    public string DatabaseName { get; init; } = "fcg_catalog";

    public string ReviewsCollectionName { get; init; } = "game_reviews";
}
