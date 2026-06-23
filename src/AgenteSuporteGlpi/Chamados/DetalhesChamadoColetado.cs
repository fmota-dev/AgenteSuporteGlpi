namespace AgenteSuporteGlpi.Chamados;

public sealed record DetalhesChamadoColetado(
    int Numero,
    string Titulo,
    string Descricao,
    StatusChamado Status,
    string Prioridade,
    string? Categoria,
    string? Solicitante,
    string Responsavel,
    DateTimeOffset DataAbertura,
    DateTimeOffset DataUltimaAtualizacao,
    Uri Link);
