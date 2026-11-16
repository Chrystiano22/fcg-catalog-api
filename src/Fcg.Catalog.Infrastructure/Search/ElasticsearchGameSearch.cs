using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Fcg.Catalog.Application.Search;
using Fcg.Catalog.Domain.Common;
using Fcg.Catalog.Domain.Games;

namespace Fcg.Catalog.Infrastructure.Search;

public sealed class ElasticsearchGameSearch(HttpClient client, string index) : IGameSearch
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GameSearchResult> SearchAsync(GameSearchQuery query, CancellationToken cancellationToken)
    {
        var body = BuildQuery(query);
        try
        {
            using var response = await client.PostAsJsonAsync($"{index}/_search", body, cancellationToken);
            if (!response.IsSuccessStatusCode) throw new SearchUnavailableException();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var hits = json.RootElement.GetProperty("hits");
            var items = hits.GetProperty("hits").EnumerateArray()
                .Select(hit => hit.GetProperty("_source").Deserialize<SearchGame>(JsonOptions)!).ToArray();
            return new(hits.GetProperty("total").GetProperty("value").GetInt64(), query.Pagina, query.Tamanho, items);
        }
        catch (HttpRequestException) { throw new SearchUnavailableException(); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { throw new SearchUnavailableException(); }
    }

    public static object BuildQuery(GameSearchQuery query)
    {
        if (query.Pagina < 1 || query.Tamanho < 1 || query.Tamanho > 100 || (long)query.Pagina * query.Tamanho > 10000
            || query.PrecoMin < 0 || query.PrecoMax < 0 || query.PrecoMin > query.PrecoMax
            || query.Q?.Length > 200 || query.Categoria?.Length > 100)
            throw new DomainValidationException("Invalid search filters or pagination (maximum window: 10000, page size: 100).");
        var filters = new List<object> { new { term = new { ativo = true } } };
        if (!string.IsNullOrWhiteSpace(query.Categoria))
            filters.Add(new { term = new Dictionary<string, string> { ["categoria.keyword"] = query.Categoria.Trim().ToLowerInvariant() } });
        var price = new Dictionary<string, decimal>();
        if (query.PrecoMin.HasValue) price["gte"] = query.PrecoMin.Value;
        if (query.PrecoMax.HasValue) price["lte"] = query.PrecoMax.Value;
        if (price.Count > 0) filters.Add(new { range = new { preco = price } });
        object textQuery = string.IsNullOrWhiteSpace(query.Q) ? new { match_all = new { } } :
            new { multi_match = new { query = query.Q.Trim(), fields = new[] { "titulo^3", "descricao", "categoria" }, fuzziness = "AUTO", @operator = "and" } };
        return new
        {
            from = (query.Pagina - 1) * query.Tamanho, size = query.Tamanho, track_total_hits = true,
            query = new { @bool = new { must = new[] { textQuery }, filter = filters } },
            sort = new object[] { new { _score = "desc" }, new { id = "asc" } }
        };
    }

    public async Task SynchronizeAsync(IReadOnlyCollection<Game> games, CancellationToken cancellationToken)
    {
        // Replaying the durable catalog also repairs missed updates after an Elasticsearch outage.
        using var exists = await client.SendAsync(new(HttpMethod.Head, index), cancellationToken);
        if (exists.StatusCode == HttpStatusCode.NotFound)
        {
            using var created = await client.PutAsJsonAsync(index, new
            {
                settings = new { number_of_shards = 1, number_of_replicas = 0,
                    analysis = new { normalizer = new { lowercase_keyword = new { type = "custom", filter = new[] { "lowercase", "asciifolding" } } } } },
                mappings = new { properties = new
                {
                    id = new { type = "keyword" }, titulo = new { type = "text", analyzer = "portuguese" },
                    descricao = new { type = "text", analyzer = "portuguese" },
                    categoria = new { type = "text", analyzer = "portuguese", fields = new { keyword = new { type = "keyword", normalizer = "lowercase_keyword" } } },
                    preco = new { type = "scaled_float", scaling_factor = 100 }, ativo = new { type = "boolean" }
                } }
            }, cancellationToken);
            if (!created.IsSuccessStatusCode && created.StatusCode != HttpStatusCode.BadRequest) created.EnsureSuccessStatusCode();
            // Another replica may have created the index; the following bulk request validates availability.
        }
        else exists.EnsureSuccessStatusCode();

        foreach (var batch in games.Chunk(500))
        {
            var bulk = new StringBuilder();
            foreach (var game in batch)
            {
                bulk.AppendLine(JsonSerializer.Serialize(new { index = new { _id = game.Id } }));
                bulk.AppendLine(JsonSerializer.Serialize(new SearchGame(game.Id, game.Title, game.Description, game.Price, game.Category, game.Active), JsonOptions));
            }
            using var content = new StringContent(bulk.ToString(), Encoding.UTF8, "application/x-ndjson");
            using var response = await client.PostAsync($"{index}/_bulk?refresh=wait_for", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (result.RootElement.GetProperty("errors").GetBoolean())
                throw new InvalidOperationException("Elasticsearch rejected catalog documents; reconciliation will retry.");
        }
    }
}
