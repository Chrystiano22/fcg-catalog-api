using Fcg.Catalog.Api.Reviews;

namespace Fcg.Catalog.Api.Contracts.Reviews;

public sealed class ReviewResponse
{
    public string Id { get; init; } = string.Empty;

    public Guid JogoId { get; init; }

    public Guid UsuarioId { get; init; }

    public int Nota { get; init; }

    public string Comentario { get; init; } = string.Empty;

    public DateTime CriadoEm { get; init; }

    public static ReviewResponse FromDocument(GameReviewDocument document)
    {
        return new ReviewResponse
        {
            Id = document.Id,
            JogoId = document.GameId,
            UsuarioId = document.UserId,
            Nota = document.Rating,
            Comentario = document.Comment,
            CriadoEm = document.CreatedAt
        };
    }
}
