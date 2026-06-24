namespace AgenteSuporteGlpi.Contratos;

public sealed record ResultadoBuscaCodigo
{
    public IReadOnlyList<ArquivoEncontrado> Arquivos { get; init; } = [];
    public int TotalMatch { get; init; }
}

public sealed record ArquivoEncontrado
{
    public required string Caminho { get; init; }
    public required string Conteudo { get; init; }
    public required string Repositorio { get; init; }
    public required string Projeto { get; init; }
}

public sealed record RepoAzureDevOps
{
    public required string Id { get; init; }
    public required string Nome { get; init; }
    public required string Projeto { get; init; }
}

public interface IBuscaCodigoFonte
{
    Task<IReadOnlyList<RepoAzureDevOps>> ListarRepositoriosAsync(CancellationToken ct);
    Task<ResultadoBuscaCodigo> BuscarCodigoAsync(
        string termoBusca,
        IReadOnlyList<RepoAzureDevOps> repos,
        int topArquivos,
        int maxLinhas,
        CancellationToken ct);
}
