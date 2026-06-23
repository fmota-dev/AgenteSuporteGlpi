using System.Text.RegularExpressions;
using AgenteSuporteGlpi.Chamados;

namespace AgenteSuporteGlpi.ColetaGlpi;

public static partial class ParserDetalhesChamado
{
    public static DetalhesChamadoColetado Converter(string html, Uri link)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(link);

        var titulo = ExtrairTag(html, "h1");
        var numero = int.Parse(ExtrairValor(html, "Numero"));
        var status = ConverterStatus(ExtrairValor(html, "Status"));
        var prioridade = ExtrairValor(html, "Prioridade");
        var categoria = ExtrairValorOpcional(html, "Categoria");
        var solicitante = ExtrairValorOpcional(html, "Solicitante");
        var responsavel = ExtrairValor(html, "Responsavel");
        var abertura = DateTimeOffset.Parse(ExtrairValor(html, "Abertura"));
        var ultimaAtualizacao = DateTimeOffset.Parse(ExtrairValor(html, "Ultima atualizacao"));
        var descricao = ExtrairPorId(html, "descricao");

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

    private static string ExtrairTag(string html, string tag) =>
        Limpar(MatchObrigatorio(html, $"<{tag}>(.*?)</{tag}>").Groups[1].Value);

    private static string ExtrairPorId(string html, string id) =>
        Limpar(MatchObrigatorio(html, $"<[^>]+id=\"{id}\"[^>]*>(.*?)</[^>]+>").Groups[1].Value);

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
