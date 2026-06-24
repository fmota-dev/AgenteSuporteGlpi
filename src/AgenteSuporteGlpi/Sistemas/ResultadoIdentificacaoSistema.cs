namespace AgenteSuporteGlpi.Sistemas;

public enum NivelConfianca
{
    NaoIdentificado = 0,
    Baixa = 1,
    Media = 2,
    Alta = 3
}

public sealed record ResultadoIdentificacaoSistema
{
    public SistemaConfigurado? Sistema { get; init; }
    public NivelConfianca Confianca { get; init; }
    public int Pontuacao { get; init; }
    public required IReadOnlyList<string> TermosEncontrados { get; init; }
    public required string Motivo { get; init; }
}
