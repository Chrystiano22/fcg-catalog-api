namespace Fcg.Catalog.Application.Search;

public sealed record GameSearchQuery(string? Q = null, string? Categoria = null,
    decimal? PrecoMin = null, decimal? PrecoMax = null, int Pagina = 1, int Tamanho = 20);

public sealed record SearchGame(Guid Id, string Titulo, string Descricao, decimal Preco, string Categoria, bool Ativo);
public sealed record GameSearchResult(long Total, int Pagina, int Tamanho, IReadOnlyList<SearchGame> Itens);

public interface IGameSearch
{
    Task<GameSearchResult> SearchAsync(GameSearchQuery query, CancellationToken cancellationToken);
}

public sealed class SearchUnavailableException : Exception
{
    public SearchUnavailableException() : base("Catalog search is temporarily unavailable.") { }
}
