using Microsoft.Extensions.Configuration;

namespace AgenteSuporteGlpi.Configuracao;

public sealed record ConfiguracaoDevOps
{
    public required string OrgUrl { get; init; }
    public required string Pat { get; init; }
    public string? ProjetoPadrao { get; init; }
    public int TopArquivos { get; init; } = 10;
    public int MaxLinhasPorArquivo { get; init; } = 500;

    public static ConfiguracaoDevOps Carregar(IConfiguration configuration)
    {
        var secao = configuration.GetSection("AzureDevOps");
        var config = secao.Get<ConfiguracaoDevOps>()
            ?? throw new InvalidOperationException("Secao 'AzureDevOps' nao encontrada ou invalida em appsettings.json.");

        if (string.IsNullOrWhiteSpace(config.OrgUrl))
            throw new InvalidOperationException("AzureDevOps:OrgUrl e obrigatorio.");

        if (string.IsNullOrWhiteSpace(config.Pat) || config.Pat.Contains("<"))
            throw new InvalidOperationException(
                "AzureDevOps:Pat nao configurado. Use 'dotnet user-secrets set AzureDevOps:Pat <token>'.");

        if (config.TopArquivos < 1 || config.TopArquivos > 10)
            throw new InvalidOperationException("AzureDevOps:TopArquivos deve estar entre 1 e 10.");

        if (config.MaxLinhasPorArquivo < 50 || config.MaxLinhasPorArquivo > 500)
            throw new InvalidOperationException("AzureDevOps:MaxLinhasPorArquivo deve estar entre 50 e 500.");

        return config;
    }
}
