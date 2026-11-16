using System.Net;
using System.Text.Json;
using Fcg.Catalog.Application.Search;
using Fcg.Catalog.Domain.Common;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Infrastructure.Search;

namespace Fcg.Catalog.UnitTests.Search;

public sealed class ElasticsearchGameSearchTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public void InvalidWindowIsRejected(int page, int size) => Assert.Throws<DomainValidationException>(() =>
        ElasticsearchGameSearch.BuildQuery(new(Pagina: page, Tamanho: size)));

    [Fact]
    public void InvertedPriceRangeIsRejected() => Assert.Throws<DomainValidationException>(() =>
        ElasticsearchGameSearch.BuildQuery(new(PrecoMin: 20, PrecoMax: 10)));

    [Fact]
    public void QueryUsesFullTextAndActiveFilter()
    {
        var json = JsonSerializer.SerializeToElement(ElasticsearchGameSearch.BuildQuery(new("aventura", "RPG", 10, 50, 2, 5)));
        Assert.Equal(5, json.GetProperty("from").GetInt32());
        var boolean = json.GetProperty("query").GetProperty("bool");
        Assert.Equal("aventura", boolean.GetProperty("must")[0].GetProperty("multi_match").GetProperty("query").GetString());
        Assert.True(boolean.GetProperty("filter")[0].GetProperty("term").GetProperty("ativo").GetBoolean());
        Assert.Equal("rpg", boolean.GetProperty("filter")[1].GetProperty("term").GetProperty("categoria.keyword").GetString());
    }

    [Fact]
    public async Task UnavailableEngineDoesNotReturnEmptySuccess()
    {
        var search = Create(new StubHandler(_ => new(HttpStatusCode.ServiceUnavailable)));
        await Assert.ThrowsAsync<SearchUnavailableException>(() => search.SearchAsync(new(), default));
    }

    [Fact]
    public async Task SearchDeserializesTotalAndDocuments()
    {
        var game = new SearchGame(Guid.NewGuid(), "Aventura", "Exploracao", 10, "RPG", true);
        var json = JsonSerializer.Serialize(new { hits = new { total = new { value = 42 }, hits = new[] { new { _source = game } } } });
        var result = await Create(new StubHandler(_ => new(HttpStatusCode.OK) { Content = new StringContent(json) })).SearchAsync(new(), default);
        Assert.Equal(42, result.Total);
        Assert.Equal(game.Id, Assert.Single(result.Itens).Id);
    }

    [Fact]
    public async Task SynchronizationIncludesDeactivatedGamesAndDetectsPartialBulkFailure()
    {
        var game = Game.Create("Adventure", "Explore", 10, "RPG");
        game.Deactivate();
        var handler = new StubHandler(request => new(HttpStatusCode.OK)
        {
            Content = new StringContent(request.Method == HttpMethod.Head ? "" : "{\"errors\":true}")
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => Create(handler).SynchronizeAsync(new[] { game }, default));
        Assert.Contains("\"ativo\":false", handler.LastBody);
        Assert.Contains(game.Id.ToString(), handler.LastBody);
    }

    private static ElasticsearchGameSearch Create(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9200") }, "test-games");

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        public string? LastBody { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return response(request);
        }
    }
}
