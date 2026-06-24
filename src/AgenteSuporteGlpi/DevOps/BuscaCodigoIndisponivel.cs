using AgenteSuporteGlpi.Contratos;

namespace AgenteSuporteGlpi.DevOps;

public sealed class BuscaCodigoIndisponivel : IBuscaCodigoFonte
{
    public Task<IReadOnlyList<RepoAzureDevOps>> ListarRepositoriosAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<RepoAzureDevOps>>([]);

    public Task<ResultadoBuscaCodigo> BuscarCodigoAsync(
        string termoBusca,
        IReadOnlyList<RepoAzureDevOps> repos,
        int topArquivos,
        int maxLinhas,
        CancellationToken ct)
        => Task.FromResult(new ResultadoBuscaCodigo { Arquivos = [], TotalMatch = 0 });
}
