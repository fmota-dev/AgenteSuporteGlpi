using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AgenteSuporteGlpi.Configuracao;
using AgenteSuporteGlpi.Contratos;

namespace AgenteSuporteGlpi.DevOps;

public sealed class BuscadorCodigoAzureDevOps : IBuscaCodigoFonte
{
    private readonly HttpClient _http;
    private readonly ConfiguracaoDevOps _config;

    public BuscadorCodigoAzureDevOps(HttpClient http, ConfiguracaoDevOps config)
    {
        _http = http;
        _config = config;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                Encoding.ASCII.GetBytes($":{config.Pat}")));
    }

    public async Task<IReadOnlyList<RepoAzureDevOps>> ListarRepositoriosAsync(CancellationToken ct)
    {
        var orgName = ExtrairOrgName(_config.OrgUrl);
        var url = $"https://dev.azure.com/{orgName}/_apis/git/repositories?api-version=7.1";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var repos = new List<RepoAzureDevOps>();

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("value", out var items))
            return repos;

        foreach (var item in items.EnumerateArray())
        {
            var id = item.GetProperty("id").GetString() ?? "";
            var nome = item.GetProperty("name").GetString() ?? "";
            var projeto = item.TryGetProperty("project", out var prj) &&
                prj.TryGetProperty("name", out var pname) ? pname.GetString() ?? "" : "";

            repos.Add(new RepoAzureDevOps { Id = id, Nome = nome, Projeto = projeto });
        }

        return repos;
    }

    public async Task<ResultadoBuscaCodigo> BuscarCodigoAsync(
        string termoBusca,
        IReadOnlyList<RepoAzureDevOps> repos,
        int topArquivos,
        int maxLinhas,
        CancellationToken ct)
    {
        var orgName = ExtrairOrgName(_config.OrgUrl);
        var projeto = repos.FirstOrDefault()?.Projeto ?? _config.ProjetoPadrao ?? "";

        var searchBody = new
        {
            searchText = termoBusca,
            skip = 0,
            top = topArquivos,
            filters = new
            {
                Repository = new
                {
                    repository = repos.Select(r => r.Nome).ToArray()
                }
            }
        };

        var url = $"https://almsearch.dev.azure.com/{orgName}/{projeto}/_apis/search/codesearchresults?api-version=7.1";

        var response = await _http.PostAsJsonAsync(url, searchBody, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var arquivos = new List<ArquivoEncontrado>();

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("results", out var results))
            return new ResultadoBuscaCodigo { Arquivos = arquivos, TotalMatch = 0 };

        foreach (var result in results.EnumerateArray())
        {
            var caminho = result.TryGetProperty("path", out var p) ? p.GetString() ?? "" : "";
            var repo = result.TryGetProperty("repository", out var r) &&
                r.TryGetProperty("name", out var rn) ? rn.GetString() ?? "" : "";
            var proj = result.TryGetProperty("project", out var pj) &&
                pj.TryGetProperty("name", out var pjn) ? pjn.GetString() ?? "" : "";

            var conteudo = "";
            if (result.TryGetProperty("content", out var c))
                conteudo = c.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(conteudo) && !string.IsNullOrWhiteSpace(caminho))
            {
                conteudo = await BaixarConteudoArquivoAsync(orgName, proj, repo, caminho, maxLinhas, ct);
            }

            if (!string.IsNullOrWhiteSpace(conteudo))
            {
                conteudo = TruncarLinhas(conteudo, maxLinhas);
                arquivos.Add(new ArquivoEncontrado
                {
                    Caminho = caminho,
                    Conteudo = conteudo,
                    Repositorio = repo,
                    Projeto = proj
                });
            }
        }

        return new ResultadoBuscaCodigo { Arquivos = arquivos, TotalMatch = results.GetArrayLength() };
    }

    private async Task<string?> BaixarConteudoArquivoAsync(
        string org, string projeto, string repo, string caminho, int maxLinhas, CancellationToken ct)
    {
        try
        {
            var url = $"https://dev.azure.com/{org}/{projeto}/_apis/git/repositories/{repo}/items" +
                $"?path={Uri.EscapeDataString(caminho)}&api-version=7.1";

            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var conteudo = await response.Content.ReadAsStringAsync(ct);
            return TruncarLinhas(conteudo, maxLinhas);
        }
        catch
        {
            return null;
        }
    }

    private static string TruncarLinhas(string conteudo, int maxLinhas)
    {
        var linhas = conteudo.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        if (linhas.Length <= maxLinhas)
            return conteudo;

        return string.Join('\n', linhas.Take(maxLinhas)) +
            $"\n// ... (truncado: {linhas.Length - maxLinhas} linhas omitidas)";
    }

    private static string ExtrairOrgName(string orgUrl)
    {
        var uri = new Uri(orgUrl.TrimEnd('/'));
        return uri.Segments.Length >= 2 ? uri.Segments[1].Trim('/') : uri.Host.Split('.')[0];
    }
}
