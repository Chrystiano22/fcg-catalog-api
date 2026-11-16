using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Fcg.Catalog.Application.Search;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Infrastructure.Search;

namespace Fcg.Catalog.Api.Search;

public static class CatalogSearchExtensions
{
    public static IServiceCollection AddCatalogSearch(this IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("Elasticsearch:Enabled"))
            return services.AddSingleton<IGameSearch, DisabledGameSearch>();

        var index = configuration["Elasticsearch:IndexName"] ?? "fcg-games-v1";
        if (!Regex.IsMatch(index, "^[a-z][a-z0-9-]{0,100}$")) throw new InvalidOperationException("Invalid Elasticsearch index name.");
        var endpoint = new Uri(configuration["Elasticsearch:Url"] ?? "http://localhost:9200");
        if (endpoint.Scheme is not ("http" or "https")) throw new InvalidOperationException("Invalid Elasticsearch URL.");
        services.AddHttpClient("elasticsearch", client =>
        {
            client.BaseAddress = endpoint;
            client.Timeout = TimeSpan.FromSeconds(15);
            var apiKey = configuration["Elasticsearch:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", apiKey);
        });
        services.AddTransient(provider => new ElasticsearchGameSearch(provider.GetRequiredService<IHttpClientFactory>().CreateClient("elasticsearch"), index));
        services.AddTransient<IGameSearch>(provider => provider.GetRequiredService<ElasticsearchGameSearch>());
        services.AddHostedService<CatalogSearchWorker>();
        return services;
    }

    private sealed class DisabledGameSearch : IGameSearch
    {
        public Task<GameSearchResult> SearchAsync(GameSearchQuery query, CancellationToken cancellationToken) => throw new SearchUnavailableException();
    }
}

public sealed class CatalogSearchWorker(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<CatalogSearchWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Clamp(configuration.GetValue("Elasticsearch:SyncIntervalSeconds", 10), 2, 3600));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var games = await scope.ServiceProvider.GetRequiredService<IGameRepository>().ListAsync(stoppingToken);
                await scope.ServiceProvider.GetRequiredService<ElasticsearchGameSearch>().SynchronizeAsync(games, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { logger.LogWarning(exception, "Catalog search synchronization failed; retrying in {Interval}", interval); }
            await Task.Delay(interval, stoppingToken);
        }
    }
}
