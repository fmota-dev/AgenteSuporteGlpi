using System.Text;
using AgenteSuporteGlpi.Chamados;
using AgenteSuporteGlpi.Sistemas;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenteSuporteGlpi.IA;

public sealed class AnalisadorChamado(AIAgent agente, ConfiguracaoIa configuracao)
{
    public async Task<ResultadoAnaliseIa> AnalisarAsync(
        ContextoAnaliseChamado contexto,
        CancellationToken cancellationToken)
    {
        var prompt = ConstruirPrompt(contexto);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(configuracao.TimeoutAgente);

        var sessao = await agente.CreateSessionAsync(cancellationToken: cts.Token);

        var resposta = await agente.RunAsync(
            prompt,
            sessao,
            cancellationToken: cts.Token);

        var texto = resposta.Messages.LastOrDefault()?.Text ?? "";
        return InterpretarResposta(texto, contexto);
    }

    private static string ConstruirPrompt(ContextoAnaliseChamado contexto)
    {
        var c = contexto.Chamado;
        var id = contexto.Identificacao;

        var sb = new StringBuilder();
        sb.AppendLine("### Chamado GLPI ###");
        sb.AppendLine($"Numero: #{c.Numero}");
        sb.AppendLine($"Titulo: {c.Titulo}");
        sb.AppendLine($"Descricao: {c.Descricao}");
        sb.AppendLine($"Status: {c.Status}");
        sb.AppendLine($"Prioridade: {c.Prioridade}");
        sb.AppendLine($"Solicitante: {c.Solicitante ?? "Nao informado"}");
        sb.AppendLine($"Responsavel: {c.Responsavel}");
        sb.AppendLine($"Data Abertura: {c.DataAbertura:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("### Identificacao do Sistema ###");

        if (id.Confianca == NivelConfianca.NaoIdentificado)
        {
            sb.AppendLine("Sistema: NAO IDENTIFICADO");
            sb.AppendLine($"Motivo: {id.Motivo}");
            sb.AppendLine("AVISO: O sistema nao foi identificado deterministicamente. " +
                "Analise o chamado pelo conteudo e sugira um sistema provavel se possivel.");
        }
        else
        {
            sb.AppendLine($"Sistema: {id.Sistema?.Nome ?? "?"}");
            sb.AppendLine($"Confianca: {id.Confianca} ({id.Pontuacao} pontos)");
            sb.AppendLine($"Termos encontrados: {string.Join(", ", id.TermosEncontrados)}");
            sb.AppendLine($"Motivo: {id.Motivo}");
        }

        if (!string.IsNullOrWhiteSpace(contexto.ContextoBanco))
        {
            sb.AppendLine();
            sb.AppendLine("### Contexto do Banco de Dados ###");
            sb.AppendLine(contexto.ContextoBanco);
        }

        if (!string.IsNullOrWhiteSpace(contexto.ContextoCodigo))
        {
            sb.AppendLine();
            sb.AppendLine("### Contexto do Codigo Fonte ###");
            sb.AppendLine(contexto.ContextoCodigo);
        }

        sb.AppendLine();
        sb.AppendLine("### Tarefa ###");
        sb.AppendLine("Analise o chamado acima e produza APENAS o seguinte formato JSON:");
        sb.AppendLine();
        sb.AppendLine("{");
        sb.AppendLine("  \"resumo_tecnico\": \"resumo curto e tecnico do problema (1-2 frases)\",");
        sb.AppendLine("  \"possivel_causa\": \"causa raiz mais provavel com base no codigo e contexto\",");
        sb.AppendLine("  \"possivel_solucao\": \"solucao tecnica sugerida com base no codigo e contexto\",");
        sb.AppendLine("  \"perguntas_solicitante\": [\"pergunta 1\", \"pergunta 2\"],");
        sb.AppendLine("  \"proximos_passos\": [\"passo 1\", \"passo 2\"]");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("Regras:");
        sb.AppendLine("- O resumo deve ser tecnico e direto.");
        sb.AppendLine("- possivel_causa: analise o codigo fonte (se disponivel) e o contexto para inferir a causa raiz. Seja especifico.");
        sb.AppendLine("- possivel_solucao: sugira uma solucao concreta baseada no codigo. Mencione arquivos ou classes se relevante.");
        sb.AppendLine("- As perguntas sao para o solicitante esclarecer o problema.");
        sb.AppendLine("- Os proximos passos sao acoes tecnicas recomendadas.");
        sb.AppendLine("- Responda SOMENTE o JSON, sem texto adicional.");

        return sb.ToString();
    }

    private static ResultadoAnaliseIa InterpretarResposta(string resposta, ContextoAnaliseChamado _)
    {
        try
        {
            var json = ExtrairJson(resposta);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            var resumo = root.TryGetProperty("resumo_tecnico", out var r) ? r.GetString() ?? "" : "";
            var possivelCausa = root.TryGetProperty("possivel_causa", out var pc) ? pc.GetString() : null;
            var possivelSolucao = root.TryGetProperty("possivel_solucao", out var ps) ? ps.GetString() : null;
            var perguntas = root.TryGetProperty("perguntas_solicitante", out var p)
                ? p.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                : [];
            var passos = root.TryGetProperty("proximos_passos", out var pp)
                ? pp.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                : [];

            return new ResultadoAnaliseIa
            {
                ResumoTecnico = resumo,
                PossivelCausa = possivelCausa,
                PossivelSolucao = possivelSolucao,
                PerguntasSolicitante = perguntas.AsReadOnly(),
                ProximosPassos = passos.AsReadOnly()
            };
        }
        catch
        {
            return new ResultadoAnaliseIa
            {
                ResumoTecnico = resposta,
                PerguntasSolicitante = [],
                ProximosPassos = []
            };
        }
    }

    private static string ExtrairJson(string resposta)
    {
        var inicio = resposta.IndexOf('{');
        var fim = resposta.LastIndexOf('}');

        if (inicio >= 0 && fim > inicio)
        {
            return resposta[inicio..(fim + 1)];
        }

        return "{}";
    }
}
