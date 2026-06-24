namespace AgenteSuporteGlpi.IA;

public sealed record ResultadoAnaliseIa
{
    public required string ResumoTecnico { get; init; }
    public string? PossivelCausa { get; init; }
    public string? PossivelSolucao { get; init; }
    public required IReadOnlyList<string> PerguntasSolicitante { get; init; }
    public required IReadOnlyList<string> ProximosPassos { get; init; }
}
