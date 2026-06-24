using System.Text.RegularExpressions;
using AgenteSuporteGlpi.Chamados;

namespace AgenteSuporteGlpi.ColetaGlpi;

public static partial class ParserDetalhesChamado
{
    public static DetalhesChamadoColetado Converter(string html, Uri link)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(link);

        var titulo = ExtrairTitulo(html);
        var numero = int.Parse(ExtrairValorFallback(html, "Numero")
            ?? throw new InvalidOperationException("Campo obrigatorio nao encontrado: Numero."));
        var status = ConverterStatus(
            ExtrairStatusHeader(html) ?? ExtrairValor(html, "Status"));
        var prioridade = ExtrairValor(html, "Prioridade");
        var categoria = ExtrairValorOpcional(html, "Categoria");
        var solicitante = ExtrairValorOpcional(html, "Solicitante");
        var responsavel = ExtrairValor(html, "Responsavel");
        var abertura = DateTimeOffset.Parse(
            ExtrairDataTooltip(html, "Criado") ?? ExtrairValor(html, "Abertura"));
        var ultimaAtualizacao = DateTimeOffset.Parse(
            ExtrairDataTooltip(html, "Última") ?? ExtrairValor(html, "Ultima atualizacao"));
        var descricao = ExtrairDescricao(html);

        return new DetalhesChamadoColetado(
            numero,
            titulo,
            descricao,
            status,
            prioridade,
            categoria,
            solicitante,
            responsavel,
            abertura,
            ultimaAtualizacao,
            link);
    }

    public static DetalhesChamadoColetado Converter(string html, ChamadoColetado chamado)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(chamado);

        var titulo = ExtrairTitulo(html);
        var numero = int.Parse(
            ExtrairValorFallback(html, "Numero") ?? chamado.Numero.ToString());
        var status = ConverterStatus(
            ExtrairStatusHeader(html) ?? ExtrairValorFallback(html, "Status") ?? chamado.Status.ToString());
        var prioridade = ExtrairValorFallback(html, "Prioridade") ?? chamado.Prioridade;
        var categoria = ExtrairValorOpcional(html, "Categoria");
        var solicitante = ExtrairValorOpcional(html, "Solicitante");
        var responsavel = ExtrairValorFallback(html, "Responsavel") ?? chamado.Responsavel;
        var abertura = DateTimeOffset.Parse(
            ExtrairDataTooltip(html, "Criado") ?? ExtrairValorFallback(html, "Abertura") ?? chamado.DataUltimaAtualizacao.ToString("O"));
        var ultimaAtualizacao = DateTimeOffset.Parse(
            ExtrairDataTooltip(html, "Última") ?? ExtrairValorFallback(html, "Ultima atualizacao") ?? chamado.DataUltimaAtualizacao.ToString("O"));
        var descricao = ExtrairDescricao(html);

        return new DetalhesChamadoColetado(
            numero,
            titulo,
            descricao,
            status,
            prioridade,
            categoria,
            solicitante,
            responsavel,
            abertura,
            ultimaAtualizacao,
            chamado.Link);
    }

    private static string ExtrairTitulo(string html)
    {
        var match = Regex.Match(html, @"<h3[^>]*class=""[^""]*navigationheader-title[^""]*""[^>]*>(.*?)</h3>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var titulo = Limpar(match.Groups[1].Value);
            titulo = Regex.Replace(titulo, @"\s*\(\d+\)\s*$", string.Empty);
            return titulo;
        }

        return ExtrairTag(html, "h1");
    }

    private static string ExtrairDescricao(string html)
    {
        var richMatch = Regex.Match(html, @"<div[^>]*class=""[^""]*rich_text_container[^""]*""[^>]*>(.*?)</div>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (richMatch.Success)
            return Limpar(richMatch.Groups[1].Value);

        return ExtrairPorId(html, "descricao");
    }

    private static string? ExtrairStatusHeader(string html)
    {
        var ariaMatch = Regex.Match(html, @"aria-label=""([^""]*(?:Novo|Em atendimento|Pendente|Solucionado|Fechado)[^""]*)""",
            RegexOptions.IgnoreCase);
        if (ariaMatch.Success)
            return ariaMatch.Groups[1].Value;

        var titleMatch = Regex.Match(html, @"data-bs-original-title=""([^""]*(?:Novo|Em atendimento|Pendente|Solucionado|Fechado)[^""]*)""",
            RegexOptions.IgnoreCase);
        if (titleMatch.Success)
            return titleMatch.Groups[1].Value;

        return null;
    }

    private static string? ExtrairDataTooltip(string html, string rotulo)
    {
        var match = Regex.Match(html,
            $@"{Regex.Escape(rotulo)}.*?data-bs-original-title=""([^""]+)""",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtrairValorFallback(string html, string rotulo)
    {
        var match = Regex.Match(html,
            $"<dt>{Regex.Escape(rotulo)}</dt>\\s*<dd>(.*?)</dd>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);
        return match.Success ? Limpar(match.Groups[1].Value) : null;
    }

    private static string ExtrairTag(string html, string tag) =>
        Limpar(MatchObrigatorio(html, $"<{tag}>(.*?)</{tag}>").Groups[1].Value);

    private static string ExtrairPorId(string html, string id) =>
        Limpar(MatchObrigatorio(html, $"<([a-zA-Z]+)[^>]*id=\"{id}\"[^>]*>(.*?)</\\1>").Groups[2].Value);

    private static string ExtrairValor(string html, string rotulo) =>
        ExtrairValorOpcional(html, rotulo) ?? throw new InvalidOperationException($"Campo obrigatorio nao encontrado: {rotulo}.");

    private static string? ExtrairValorOpcional(string html, string rotulo)
    {
        var match = Regex.Match(html, $"<dt>{Regex.Escape(rotulo)}</dt>\\s*<dd>(.*?)</dd>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        return match.Success ? Limpar(match.Groups[1].Value) : null;
    }

    private static Match MatchObrigatorio(string html, string pattern)
    {
        var match = Regex.Match(html, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        return match.Success ? match : throw new InvalidOperationException("HTML do chamado nao contem estrutura esperada.");
    }

    private static string Limpar(string valor) => TagsHtml().Replace(valor, string.Empty).Trim();

    private static StatusChamado ConverterStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "novo" => StatusChamado.Novo,
        "em atendimento" => StatusChamado.EmAtendimento,
        "pendente" => StatusChamado.Pendente,
        "solucionado" => StatusChamado.Solucionado,
        "fechado" => StatusChamado.Fechado,
        _ => StatusChamado.Desconhecido
    };

    [GeneratedRegex("<.*?>")]
    private static partial Regex TagsHtml();
}
