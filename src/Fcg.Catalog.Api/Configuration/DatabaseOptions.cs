using System.ComponentModel.DataAnnotations;

namespace Fcg.Catalog.Api.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string Provider { get; init; } = "Sqlite";

    public string? ConnectionString { get; init; }
}
