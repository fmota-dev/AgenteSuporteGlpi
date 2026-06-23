namespace AgenteSuporteGlpi.Execucoes;

public sealed record ExecucaoColeta(
    long Id,
    DateTimeOffset Inicio,
    DateTimeOffset? Fim,
    string Status,
    int QuantidadeEncontrada,
    int QuantidadeColetada,
    int QuantidadeIgnorada,
    int QuantidadeComErro,
    string? MensagemErro);
