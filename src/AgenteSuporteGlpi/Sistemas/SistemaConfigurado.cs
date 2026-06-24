namespace AgenteSuporteGlpi.Sistemas;

public sealed record SistemaConfigurado
{
    public required string Nome { get; init; }
    public bool Ativo { get; init; } = true;
    public required IReadOnlyList<string> Aliases { get; init; }
    public IReadOnlyList<string> PalavrasChave { get; init; } = [];
    public IReadOnlyList<string> Repositorios { get; init; } = [];
    public IReadOnlyList<string> Bancos { get; init; } = [];
    public string? Observacoes { get; init; }
}
