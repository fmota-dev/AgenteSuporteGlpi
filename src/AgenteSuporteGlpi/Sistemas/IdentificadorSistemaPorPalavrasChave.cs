using System.Globalization;
using System.Text;

namespace AgenteSuporteGlpi.Sistemas;

public static class IdentificadorSistemaPorPalavrasChave
{
    private const int PesoAliasTitulo = 10;
    private const int PesoAliasDescricao = 7;
    private const int PesoPalavraChaveTitulo = 5;
    private const int PesoPalavraChaveDescricao = 3;

    public static ResultadoIdentificacaoSistema Identificar(
        string titulo,
        string descricao,
        IReadOnlyList<SistemaConfigurado> sistemas)
    {
        var tituloNormalizado = Normalizar(titulo);
        var descricaoNormalizada = Normalizar(descricao);

        var sistemasAtivos = sistemas.Where(s => s.Ativo).ToList();

        if (sistemasAtivos.Count == 0)
        {
            return new ResultadoIdentificacaoSistema
            {
                Confianca = NivelConfianca.NaoIdentificado,
                TermosEncontrados = [],
                Motivo = "Nenhum sistema ativo configurado."
            };
        }

        var pontuacoes = new List<(SistemaConfigurado Sistema, int Pontuacao, List<string> Termos)>();

        foreach (var sistema in sistemasAtivos)
        {
            var pontuacao = 0;
            var termos = new List<string>();

            foreach (var alias in sistema.Aliases ?? [])
            {
                var aliasNormalizado = Normalizar(alias);
                if (tituloNormalizado.Contains(aliasNormalizado, StringComparison.OrdinalIgnoreCase))
                {
                    pontuacao += PesoAliasTitulo;
                    termos.Add($"alias '{alias}' no titulo");
                }
                else if (descricaoNormalizada.Contains(aliasNormalizado, StringComparison.OrdinalIgnoreCase))
                {
                    pontuacao += PesoAliasDescricao;
                    termos.Add($"alias '{alias}' na descricao");
                }
            }

            foreach (var palavra in sistema.PalavrasChave ?? [])
            {
                var palavraNormalizada = Normalizar(palavra);
                if (tituloNormalizado.Contains(palavraNormalizada, StringComparison.OrdinalIgnoreCase))
                {
                    pontuacao += PesoPalavraChaveTitulo;
                    termos.Add($"palavra-chave '{palavra}' no titulo");
                }
                else if (descricaoNormalizada.Contains(palavraNormalizada, StringComparison.OrdinalIgnoreCase))
                {
                    pontuacao += PesoPalavraChaveDescricao;
                    termos.Add($"palavra-chave '{palavra}' na descricao");
                }
            }

            if (pontuacao > 0)
            {
                pontuacoes.Add((sistema, pontuacao, termos));
            }
        }

        if (pontuacoes.Count == 0)
        {
            return new ResultadoIdentificacaoSistema
            {
                Confianca = NivelConfianca.NaoIdentificado,
                TermosEncontrados = [],
                Motivo = "Nenhum termo conhecido encontrado no titulo ou descricao."
            };
        }

        pontuacoes.Sort((a, b) => b.Pontuacao.CompareTo(a.Pontuacao));

        if (pontuacoes.Count > 1 && pontuacoes[0].Pontuacao == pontuacoes[1].Pontuacao)
        {
            var empatados = pontuacoes.TakeWhile(p => p.Pontuacao == pontuacoes[0].Pontuacao).ToList();
            var nomes = empatados.Select(p => $"'{p.Sistema.Nome}'").ToList();
            return new ResultadoIdentificacaoSistema
            {
                Confianca = NivelConfianca.Baixa,
                TermosEncontrados = empatados.SelectMany(p => p.Termos).Distinct().ToList(),
                Motivo = $"Empate entre sistemas: {string.Join(", ", nomes)}. Pontuacao: {pontuacoes[0].Pontuacao}."
            };
        }

        var melhor = pontuacoes[0];
        var confianca = melhor.Pontuacao >= PesoAliasTitulo
            ? NivelConfianca.Alta
            : melhor.Pontuacao >= PesoAliasDescricao
                ? NivelConfianca.Media
                : NivelConfianca.Baixa;

        var sbMotivo = new StringBuilder();
        sbMotivo.Append("Termos encontrados: ");
        sbMotivo.Append(string.Join("; ", melhor.Termos));
        sbMotivo.Append($". Pontuacao total: {melhor.Pontuacao}.");

        return new ResultadoIdentificacaoSistema
        {
            Sistema = melhor.Sistema,
            Confianca = confianca,
            Pontuacao = melhor.Pontuacao,
            TermosEncontrados = melhor.Termos,
            Motivo = sbMotivo.ToString()
        };
    }

    private static string Normalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return string.Empty;

        var textoNormalizado = RemoverAcentos(texto);
        return textoNormalizado.ToLowerInvariant();
    }

    private static string RemoverAcentos(string texto)
    {
        var normalized = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
