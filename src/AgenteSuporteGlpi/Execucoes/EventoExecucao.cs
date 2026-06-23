namespace AgenteSuporteGlpi.Execucoes;

public sealed record EventoExecucao(
    long Id,
    long? ExecucaoId,
    DateTimeOffset DataHora,
    string Nivel,
    string Etapa,
    string Mensagem,
    int? NumeroChamado);
