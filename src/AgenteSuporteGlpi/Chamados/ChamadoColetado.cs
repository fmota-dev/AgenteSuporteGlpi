namespace AgenteSuporteGlpi.Chamados;

public sealed record ChamadoColetado(
    int Numero,
    string Titulo,
    StatusChamado Status,
    string Prioridade,
    string Responsavel,
    DateTimeOffset DataUltimaAtualizacao,
    Uri Link);
