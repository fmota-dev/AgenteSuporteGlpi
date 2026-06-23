using AgenteSuporteGlpi.Chamados;
using AgenteSuporteGlpi.Configuracao;
using AgenteSuporteGlpi.Contratos;
using Microsoft.Playwright;

namespace AgenteSuporteGlpi.ColetaGlpi;

public sealed class ColetorGlpiPlaywright : IColetorGlpi
{
    private readonly ConfiguracaoGlpi _configuracaoGlpi;
    private readonly ConfiguracaoBrowser _configuracaoBrowser;
    private readonly SeletoresGlpi _seletores;

    public ColetorGlpiPlaywright(ConfiguracaoGlpi configuracaoGlpi, ConfiguracaoBrowser configuracaoBrowser, SeletoresGlpi seletores)
    {
        _configuracaoGlpi = configuracaoGlpi;
        _configuracaoBrowser = configuracaoBrowser;
        _seletores = seletores;
    }

    public async Task<IReadOnlyList<ChamadoColetado>> ColetarListaAsync(CancellationToken ct)
    {
        var resultados = new List<ChamadoColetado>(_configuracaoGlpi.LimiteChamadosPorExecucao);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = _configuracaoBrowser.Headless,
        });

        var page = await AbrirPaginaAutenticadaAsync(browser, ct);

        foreach (var statusId in _configuracaoGlpi.StatusParaColetar)
        {
            if (resultados.Count >= _configuracaoGlpi.LimiteChamadosPorExecucao)
                break;

            ct.ThrowIfCancellationRequested();

            var urlFiltro = ConstruirUrlFiltro(statusId);
            await page.GotoAsync(urlFiltro, new() { WaitUntil = WaitUntilState.NetworkIdle });
            await BloquearFluxosInesperadosAsync(page);

            bool temProxima;
            do
            {
                temProxima = false;
                var linhas = await page.Locator(_seletores.LinhaChamado).AllAsync();

                foreach (var linha in linhas)
                {
                    if (resultados.Count >= _configuracaoGlpi.LimiteChamadosPorExecucao)
                        break;

                    ct.ThrowIfCancellationRequested();

                    var chamado = await ExtrairChamadoDaLinhaAsync(linha, _configuracaoGlpi.UrlBase);
                    if (chamado is not null)
                        resultados.Add(chamado);
                }

                var proximo = page.Locator(_seletores.LinkProximaPagina);
                if (await proximo.CountAsync() > 0 && resultados.Count < _configuracaoGlpi.LimiteChamadosPorExecucao)
                {
                    await proximo.First.ClickAsync();
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    temProxima = true;
                }
            } while (temProxima && resultados.Count < _configuracaoGlpi.LimiteChamadosPorExecucao);
        }

        return resultados;
    }

    public async Task<DetalhesChamadoColetado> ColetarDetalhesAsync(ChamadoColetado chamado, CancellationToken ct)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = _configuracaoBrowser.Headless,
        });

        var page = await AbrirPaginaAutenticadaAsync(browser, ct);

        await page.GotoAsync(chamado.Link.ToString(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await BloquearFluxosInesperadosAsync(page);

        await page.WaitForSelectorAsync(_seletores.ConteudoChamado, new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = _configuracaoBrowser.TimeoutMilissegundos,
        });

        await Task.Delay(_configuracaoBrowser.TimeoutEsperaAjaxMilissegundos, ct);

        var html = await page.Locator(_seletores.ConteudoChamado).InnerHTMLAsync();
        return ParserDetalhesChamado.Converter(html, chamado.Link);
    }

    private async Task<IPage> AbrirPaginaAutenticadaAsync(IBrowser browser, CancellationToken ct)
    {
        var page = await browser.NewPageAsync();
        page.SetDefaultTimeout(_configuracaoBrowser.TimeoutMilissegundos);

        await page.GotoAsync(_configuracaoGlpi.UrlBase.ToString(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        await BloquearFluxosInesperadosAsync(page);

        var campoUsuario = page.GetByLabel("Usuário");
        if (await campoUsuario.CountAsync() == 0)
            campoUsuario = page.Locator(_seletores.CampoUsuario);
        await campoUsuario.FillAsync(_configuracaoGlpi.UsuarioLogin);

        var campoSenha = page.GetByLabel("Senha");
        if (await campoSenha.CountAsync() == 0)
            campoSenha = page.Locator(_seletores.CampoSenha);
        await campoSenha.FillAsync(_configuracaoGlpi.SenhaLogin);

        await page.GetByRole(AriaRole.Button, new() { Name = "Entrar" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await BloquearFluxosInesperadosAsync(page);

        return page;
    }

    private static async Task BloquearFluxosInesperadosAsync(IPage page)
    {
        var body = (await page.Locator("body").TextContentAsync()) ?? string.Empty;
        var lower = body.ToLowerInvariant();

        if (lower.Contains("captcha") || lower.Contains("alterar senha") || lower.Contains("mfa"))
            throw new InvalidOperationException("Fluxo inesperado detectado no GLPI. Coleta abortada.");
    }

    private string ConstruirUrlFiltro(int statusId)
    {
        return new Uri(_configuracaoGlpi.UrlBase,
            $"/front/ticket.php?criteria[0][field]=12&criteria[0][searchtype]=equals&criteria[0][value]={statusId}&criteria[0][link]=AND&criteria[1][field]=5&criteria[1][searchtype]=equals&criteria[1][value]={_configuracaoGlpi.UserGlpiId}&criteria[1][link]=AND")
            .ToString();
    }

    private static async Task<ChamadoColetado?> ExtrairChamadoDaLinhaAsync(ILocator linha, Uri urlBase)
    {
        var celulas = await linha.Locator("td").AllAsync();
        if (celulas.Count < 8)
            return null;

        var linkId = celulas[0].Locator("a");
        var textoNumero = await linkId.CountAsync() > 0
            ? await linkId.TextContentAsync()
            : await celulas[0].TextContentAsync();
        if (string.IsNullOrWhiteSpace(textoNumero) || !int.TryParse(textoNumero.Trim(), out var numero))
            return null;

        var linkTitulo = celulas[1].Locator("a");
        if (await linkTitulo.CountAsync() == 0)
            return null;

        var titulo = (await linkTitulo.TextContentAsync())?.Trim() ?? string.Empty;
        var href = await linkTitulo.GetAttributeAsync("href");
        if (string.IsNullOrWhiteSpace(href))
            return null;

        var link = new Uri(urlBase, href);

        var statusTexto = (await celulas[2].TextContentAsync())?.Trim() ?? string.Empty;
        var status = ConverterStatusTabela(statusTexto);

        var prioridade = (await celulas[5].TextContentAsync())?.Trim() ?? string.Empty;

        var responsavel = (await celulas[7].TextContentAsync())?.Trim() ?? string.Empty;

        var dataTexto = (await celulas[3].TextContentAsync())?.Trim() ?? string.Empty;
        if (!DateTimeOffset.TryParse(dataTexto, out var dataUltimaAtualizacao))
            return null;

        return new ChamadoColetado(numero, titulo, status, prioridade, responsavel, dataUltimaAtualizacao, link);
    }

    private static StatusChamado ConverterStatusTabela(string statusTexto)
    {
        return statusTexto.Trim().ToLowerInvariant() switch
        {
            "novo" => StatusChamado.Novo,
            "em atendimento" => StatusChamado.EmAtendimento,
            "planejado" => StatusChamado.Pendente,
            "pendente" => StatusChamado.Pendente,
            "solucionado" => StatusChamado.Solucionado,
            "fechado" => StatusChamado.Fechado,
            _ => StatusChamado.Desconhecido,
        };
    }
}
