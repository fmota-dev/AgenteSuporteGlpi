namespace AgenteSuporteGlpi.Configuracao;

public sealed class ConfiguracaoGlpi
{
    public required Uri UrlBase { get; init; }
    public required string UsuarioLogin { get; init; }
    public required string SenhaLogin { get; init; }
    public required string Responsavel { get; init; }
    public required string UserGlpiId { get; init; }
    public int LimiteChamadosPorExecucao { get; init; } = 5;
    public required IReadOnlyList<int> StatusParaColetar { get; init; }
}
