namespace Fcg.Catalog.Api.Contracts.Reviews;

public sealed class CreateReviewRequest
{
    public int Nota { get; init; }

    public string Comentario { get; init; } = string.Empty;
}
