#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Extensions.Configuration.Json, 10.0.7"
#r "nuget: Microsoft.Extensions.Configuration.Binder, 10.0.7"
#r "nuget: Microsoft.Data.Sqlite, 10.0.9"

#nullable enable
using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

// --- Tipos inline (copiados do projeto principal) ---

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

public enum NivelConfianca
{
    NaoIdentificado = 0,
    Baixa = 1,
    Media = 2,
    Alta = 3
}

public sealed record ResultadoIdentificacaoSistema
{
    public SistemaConfigurado? Sistema { get; init; }
    public NivelConfianca Confianca { get; init; }
    public int Pontuacao { get; init; }
    public required IReadOnlyList<string> TermosEncontrados { get; init; }
    public required string Motivo { get; init; }
}

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
                pontuacoes.Add((sistema, pontuacao, termos));
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
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

// --- Script de validacao ---

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("src/AgenteSuporteGlpi/appsettings.json")
    .Build();

var sistemas = config.GetSection("Sistemas").Get<List<SistemaConfigurado>>()
    ?? throw new InvalidOperationException("Secao 'Sistemas' nao encontrada ou invalida.");

Console.WriteLine($"Sistemas ativos: {sistemas.Count(s => s.Ativo)}/{sistemas.Count}");
foreach (var s in sistemas.Where(x => x.Ativo))
    Console.WriteLine($"  {s.Nome}: aliases=[{string.Join(", ", s.Aliases)}] palavras=[{string.Join(", ", s.PalavrasChave)}]");
Console.WriteLine();

var cs = "Data Source=C:\\Users\\f4179\\Desktop\\AgenteSuporteGlpi\\src\\AgenteSuporteGlpi\\dados\\agente-suporte-glpi.db";
var acertos = 0;
var erros = 0;
var total = 0;

using (var conn = new SqliteConnection(cs))
{
    conn.Open();
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"
            SELECT c.Numero, c.TituloAtual, cc.DescricaoColetada
            FROM Chamados c
            JOIN ColetasChamado cc ON cc.NumeroChamado = c.Numero
            WHERE cc.Id = (SELECT MAX(Id) FROM ColetasChamado WHERE NumeroChamado = c.Numero)
            ORDER BY c.Numero DESC";
        using (var reader = cmd.ExecuteReader())
        {
            Console.WriteLine("Chamado         | Titulo                                                    | Sistema              | Confianca | Pontos");
            Console.WriteLine(new string('-', 42) + "+" + new string('-', 60) + "+" + new string('-', 22) + "+" + new string('-', 11) + "+" + new string('-', 8));

            while (reader.Read())
            {
                total++;
                var numero = reader.GetString(0);
                var titulo = reader.GetString(1);
                var descricao = reader.IsDBNull(2) ? "" : reader.GetString(2);

                var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(titulo, descricao, sistemas);

                var tituloTrunc = titulo.Length > 57 ? titulo[..54] + "..." : titulo;
                var sistemaNome = resultado.Sistema?.Nome ?? "-";
                var confianca = resultado.Confianca.ToString();

                Console.WriteLine($"#{numero,-13} | {tituloTrunc,-58} | {sistemaNome,-20} | {confianca,-9} | {resultado.Pontuacao,6}");

                if (resultado.Confianca != NivelConfianca.NaoIdentificado)
                    acertos++;
                else
                {
                    erros++;
                    Console.WriteLine($"  >> MOTIVO: {resultado.Motivo}");
                }
            }
        }
    }
}

Console.WriteLine();
Console.WriteLine($"Total: {total} chamados | Identificados: {acertos} | Nao identificados: {erros}");
Console.WriteLine($"Taxa de acerto: {(total > 0 ? (acertos * 100.0 / total) : 0):F1}%");
