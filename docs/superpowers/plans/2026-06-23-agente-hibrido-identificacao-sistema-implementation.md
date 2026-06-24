# Agente Hibrido e Identificacao de Sistema - Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar identificacao deterministica de sistema por palavras-chave sobre chamados GLPI, com configuracao via `appsettings.json`, validacao por testes de unidade e integracao ao pipeline existente.

**Architecture:** Quatro componentes de dominio em `src/AgenteSuporteGlpi/Sistemas/`: `SistemaConfigurado` (record de config), `ConfiguracaoSistemas` (carga de `IConfiguration`), `ResultadoIdentificacaoSistema` (saida do identificador), `IdentificadorSistemaPorPalavrasChave` (engine deterministico). O pipeline chama o identificador apos salvar chamado novo/alterado.

**Tech Stack:** .NET 10, xUnit v3, FluentAssertions, Microsoft.Extensions.Configuration

---

## File Structure

```
src/AgenteSuporteGlpi/
  Sistemas/
    SistemaConfigurado.cs            # record: Nome, Ativo, Aliases, PalavrasChave, Repositorios, Bancos, Observacoes
    ConfiguracaoSistemas.cs          # static class: Carregar(IConfiguration) -> IReadOnlyList<SistemaConfigurado>
    ResultadoIdentificacaoSistema.cs # record: Sistema, Confianca, Pontuacao, TermosEncontrados, Motivo
    IdentificadorSistemaPorPalavrasChave.cs # static class: Identificar(titulo, descricao, sistemas) -> ResultadoIdentificacaoSistema
  appsettings.json                   # +secao "Sistemas"
  Program.cs                         # +DI registrations + chamada apos save

tests/AgenteSuporteGlpi.Testes/
  Sistemas/
    IdentificadorSistemaPorPalavrasChaveTestes.cs  # testes de unidade
```

---

### Task 1: SistemaConfigurado record

**Files:**
- Create: `src/AgenteSuporteGlpi/Sistemas/SistemaConfigurado.cs`

- [ ] **Step 1: Write the record**

```csharp
namespace AgenteSuporteGlpi.Sistemas;

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
```

- [ ] **Step 2: Build verification**

```bash
dotnet build src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/AgenteSuporteGlpi/Sistemas/SistemaConfigurado.cs
git commit -m "feat(sistemas): add SistemaConfigurado record"
```

---

### Task 2: ConfiguracaoSistemas loader

**Files:**
- Create: `src/AgenteSuporteGlpi/Sistemas/ConfiguracaoSistemas.cs`

- [ ] **Step 1: Write the config loader**

```csharp
using Microsoft.Extensions.Configuration;

namespace AgenteSuporteGlpi.Sistemas;

public static class ConfiguracaoSistemas
{
    public static IReadOnlyList<SistemaConfigurado> Carregar(IConfiguration configuration)
    {
        var secoes = configuration.GetSection("Sistemas").GetChildren();
        var sistemas = new List<SistemaConfigurado>();

        foreach (var secao in secoes)
        {
            var sistema = secao.Get<SistemaConfigurado>()
                ?? throw new InvalidOperationException(
                    $"Sistema na secao '{secao.Path}' nao pode ser desserializado. Verifique os campos obrigatorios.");

            if (string.IsNullOrWhiteSpace(sistema.Nome))
            {
                throw new InvalidOperationException(
                    $"Sistema na secao '{secao.Path}' requer o campo 'Nome'.");
            }

            sistemas.Add(sistema);
        }

        return sistemas;
    }
}
```

- [ ] **Step 2: Build verification**

```bash
dotnet build src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/AgenteSuporteGlpi/Sistemas/ConfiguracaoSistemas.cs
git commit -m "feat(sistemas): add ConfiguracaoSistemas loader from appsettings"
```

---

### Task 3: ResultadoIdentificacaoSistema record

**Files:**
- Create: `src/AgenteSuporteGlpi/Sistemas/ResultadoIdentificacaoSistema.cs`

- [ ] **Step 1: Write the result record**

```csharp
namespace AgenteSuporteGlpi.Sistemas;

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
```

- [ ] **Step 2: Build verification**

```bash
dotnet build src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/AgenteSuporteGlpi/Sistemas/ResultadoIdentificacaoSistema.cs
git commit -m "feat(sistemas): add ResultadoIdentificacaoSistema and NivelConfianca"
```

---

### Task 4: IdentificadorSistemaPorPalavrasChave engine

**Files:**
- Create: `src/AgenteSuporteGlpi/Sistemas/IdentificadorSistemaPorPalavrasChave.cs`

- [ ] **Step 1: Write the deterministic identifier**

```csharp
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
```

- [ ] **Step 2: Build verification**

```bash
dotnet build src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/AgenteSuporteGlpi/Sistemas/IdentificadorSistemaPorPalavrasChave.cs
git commit -m "feat(sistemas): add IdentificadorSistemaPorPalavrasChave deterministic engine"
```

---

### Task 5: Add Sistemas section to appsettings.json

**Files:**
- Modify: `src/AgenteSuporteGlpi/appsettings.json`

- [ ] **Step 1: Read current appsettings.json**

Read `src/AgenteSuporteGlpi/appsettings.json`.

- [ ] **Step 2: Add Sistemas section**

Add the `"Sistemas"` section AFTER the `"Banco"` section (before the closing `}` of the root object):

```json
  "Sistemas": [
    {
      "Nome": "Sistema de Pesquisas",
      "Ativo": true,
      "Aliases": ["pesquisas", "sistema de pesquisas"],
      "PalavrasChave": ["pesquisa", "pergunta", "questionario"],
      "Repositorios": ["C:\\dev\\azure\\SistemaPesquisas"],
      "Bancos": ["SrvDb06"]
    },
    {
      "Nome": "Sistema de Agenda",
      "Ativo": true,
      "Aliases": ["agenda", "sistema de agenda"],
      "PalavrasChave": ["agenda", "agendamento", "compromisso", "evento"],
      "Repositorios": ["C:\\dev\\azure\\SistemaAgenda"],
      "Bancos": ["SrvDb01"]
    }
  ]
```

- [ ] **Step 3: Build verification**

```bash
dotnet build src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

- [ ] **Step 4: Commit**

```bash
git add src/AgenteSuporteGlpi/appsettings.json
git commit -m "feat(sistemas): add Sistemas config section with Pesquisas and Agenda"
```

---

### Task 6: Write unit tests for the identifier

**Files:**
- Create: `tests/AgenteSuporteGlpi.Testes/Sistemas/IdentificadorSistemaPorPalavrasChaveTestes.cs`

- [ ] **Step 1: Write the test class**

```csharp
using AgenteSuporteGlpi.Sistemas;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Sistemas;

public sealed class IdentificadorSistemaPorPalavrasChaveTestes
{
    private static readonly IReadOnlyList<SistemaConfigurado> SistemasPadrao =
    [
        new SistemaConfigurado
        {
            Nome = "Sistema de Pesquisas",
            Ativo = true,
            Aliases = ["pesquisas", "sistema de pesquisas"],
            PalavrasChave = ["pesquisa", "pergunta", "questionario"]
        },
        new SistemaConfigurado
        {
            Nome = "Sistema de Agenda",
            Ativo = true,
            Aliases = ["agenda", "sistema de agenda"],
            PalavrasChave = ["agenda", "agendamento", "compromisso", "evento"]
        }
    ];

    [Fact]
    public void Deve_identificar_sistema_por_alias_exato_no_titulo()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Sistema de Pesquisas - Criacao de novas perguntas",
            descricao: "Necessario criar novas perguntas no sistema.",
            sistemas: SistemasPadrao);

        resultado.Sistema.Should().NotBeNull();
        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_identificar_agenda_por_alias_no_titulo()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Agenda",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema.Should().NotBeNull();
        resultado.Sistema!.Nome.Should().Be("Sistema de Agenda");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_identificar_agenda_por_palavras_chave_na_descricao()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Responsavel nao recebe e-mails nem mensagens na agenda",
            descricao: "O responsavel nao recebe notificacoes de agendamento.",
            sistemas: SistemasPadrao);

        resultado.Sistema.Should().NotBeNull();
        resultado.Sistema!.Nome.Should().Be("Sistema de Agenda");
    }

    [Fact]
    public void Deve_identificar_pesquisas_por_palavra_chave_no_titulo()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Erro ao exportar pesquisa",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema.Should().NotBeNull();
        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
    }

    [Fact]
    public void Deve_pontuar_alias_no_titulo_mais_que_palavra_chave_na_descricao()
    {
        var sistemas = new List<SistemaConfigurado>
        {
            new() { Nome = "Sistema A", Ativo = true, Aliases = ["sistema a"], PalavrasChave = [] },
            new() { Nome = "Sistema B", Ativo = true, Aliases = [], PalavrasChave = ["sistema a"] }
        };

        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Problema no Sistema A",
            descricao: "sistema a com erro",
            sistemas: sistemas);

        resultado.Sistema!.Nome.Should().Be("Sistema A");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_retornar_nao_identificado_quando_nenhum_termo_encontrado()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Problema no Outlook",
            descricao: "Nao consigo enviar e-mails.",
            sistemas: SistemasPadrao);

        resultado.Confianca.Should().Be(NivelConfianca.NaoIdentificado);
        resultado.Sistema.Should().BeNull();
        resultado.TermosEncontrados.Should().BeEmpty();
    }

    [Fact]
    public void Deve_ignorar_sistema_inativo()
    {
        var sistemas = new List<SistemaConfigurado>
        {
            new() { Nome = "Sistema Inativo", Ativo = false, Aliases = ["inativo"], PalavrasChave = [] },
            new() { Nome = "Sistema Ativo", Ativo = true, Aliases = ["ativo"], PalavrasChave = [] }
        };

        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "inativo",
            descricao: "",
            sistemas: sistemas);

        resultado.Confianca.Should().Be(NivelConfianca.NaoIdentificado);
    }

    [Fact]
    public void Deve_retornar_baixa_em_empate_entre_sistemas()
    {
        var sistemas = new List<SistemaConfigurado>
        {
            new() { Nome = "Sistema X", Ativo = true, Aliases = ["modulo x"], PalavrasChave = [] },
            new() { Nome = "Sistema Y", Ativo = true, Aliases = ["modulo y"], PalavrasChave = [] }
        };

        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "modulo x e modulo y",
            descricao: "",
            sistemas: sistemas);

        resultado.Confianca.Should().Be(NivelConfianca.Baixa);
        resultado.Sistema.Should().BeNull();
        resultado.Motivo.Should().Contain("Empate");
    }

    [Fact]
    public void Deve_normalizar_texto_sem_acentos_e_case_insensitive()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "SISTEMA DE PESQUISAS",
            descricao: "Questionário com erro",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_acumular_pontuacao_com_multiplos_termos_do_mesmo_sistema()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Pesquisas - nova pergunta",
            descricao: "questionario com erro",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
        resultado.Pontuacao.Should().BeGreaterThan(10);
    }

    [Fact]
    public void Deve_incluir_termos_encontrados_e_motivo_no_resultado()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Pesquisas",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.TermosEncontrados.Should().NotBeEmpty();
        resultado.Motivo.Should().NotBeNullOrWhiteSpace();
        resultado.Pontuacao.Should().BePositive();
    }

    [Fact]
    public void Deve_retornar_nao_identificado_com_lista_vazia_de_sistemas()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "qualquer coisa",
            descricao: "",
            sistemas: []);

        resultado.Confianca.Should().Be(NivelConfianca.NaoIdentificado);
    }

    [Fact]
    public void Deve_identificar_chamado_real_31905_sistema_pesquisas()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Sistema de pesquisas - Criacao de novas perguntas",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_identificar_chamado_real_31619_agenda()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Agenda",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Agenda");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_identificar_chamado_real_31868_agenda_por_palavras_chave()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Responsavel nao recebe e-mails nem mensagens na agenda",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Agenda");
    }
}
```

- [ ] **Step 2: Run tests to verify they fail/pass**

```bash
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter "FullyQualifiedName~IdentificadorSistemaPorPalavrasChaveTestes"
```

- [ ] **Step 3: Commit**

```bash
git add tests/AgenteSuporteGlpi.Testes/Sistemas/IdentificadorSistemaPorPalavrasChaveTestes.cs
git commit -m "test(sistemas): add unit tests for IdentificadorSistemaPorPalavrasChave"
```

---

### Task 7: Wire DI and call identification in Program.cs

**Files:**
- Modify: `src/AgenteSuporteGlpi/Program.cs`

- [ ] **Step 1: Read current Program.cs**

Read `src/AgenteSuporteGlpi/Program.cs` to ensure context.

- [ ] **Step 2: Add using and field for Sistemas**

Add at line 7 (after `using AgenteSuporteGlpi.Contratos;`):
```csharp
using AgenteSuporteGlpi.Sistemas;
```

Add field at line 22 (inside the class, after `_configuracaoGlpi`):
```csharp
    private readonly IdentificadorSistemaPorPalavrasChave _ = null!; // suppress unused warning
```

- [ ] **Step 3: Register config and identifier in DI**

In `ConfigureServices`, after line 145 (`services.AddSingleton(configuracaoBanco);`), add:

```csharp
                var sistemas = ConfiguracaoSistemas.Carregar(config);
                services.AddSingleton(sistemas);
```

- [ ] **Step 4: Call identifier after saving chamado**

In `StartAsync`, after the save block (after line 68 `await _repositorio.SalvarChamadoAsync(detalhes, hash, cancellationToken);`), and before the status console output, add the identification call. Replace lines 66-78 (the if/else block that handles new/altered/ignored) with:

```csharp
                    if (resultado.FoiAlterado)
                    {
                        await _repositorio.SalvarChamadoAsync(detalhes, hash, cancellationToken);

                        var identificacao = IdentificadorSistemaPorPalavrasChave.Identificar(
                            detalhes.Titulo,
                            detalhes.Descricao ?? string.Empty,
                            sistemas);

                        if (resultado.EhNovo)
                        {
                            novos++;
                            Console.WriteLine($"  [#{chamado.Numero}] NOVO - {chamado.Titulo} -> {FormatarIdentificacao(identificacao)}");
                        }
                        else
                        {
                            alterados++;
                            Console.WriteLine($"  [#{chamado.Numero}] ALTERADO - {chamado.Titulo} -> {FormatarIdentificacao(identificacao)}");
                        }
                    }
                    else
                    {
                        ignorados++;
                        Console.WriteLine($"  [#{chamado.Numero}] sem alteracoes - ignorado");
                    }
```

- [ ] **Step 5: Add FormatarIdentificacao helper method**

After the `StopAsync` method (after line 115), add:

```csharp
    private static string FormatarIdentificacao(ResultadoIdentificacaoSistema resultado)
    {
        if (resultado.Confianca == NivelConfianca.NaoIdentificado)
            return "sistema nao identificado";

        return $"{(resultado.Sistema?.Nome ?? "?")} ({resultado.Confianca}, {resultado.Pontuacao}pts)";
    }
```

- [ ] **Step 6: Resolve sistemas from DI in constructor**

Modify the constructor (line 27) to accept `IReadOnlyList<SistemaConfigurado> sistemas`:

```csharp
    public Program(
        IHostApplicationLifetime lifetime,
        IColetorGlpi coletor,
        IRepositorioChamados repositorio,
        InicializadorBanco dbInit,
        ConfiguracaoGlpi configuracaoGlpi,
        IReadOnlyList<SistemaConfigurado> sistemas)
    {
        _lifetime = lifetime;
        _coletor = coletor;
        _repositorio = repositorio;
        _dbInit = dbInit;
        _configuracaoGlpi = configuracaoGlpi;
        _sistemas = sistemas;
    }
```

Add field (replace the placeholder from Step 2):
```csharp
    private readonly IReadOnlyList<SistemaConfigurado> _sistemas;
```

- [ ] **Step 7: Build verification**

```bash
dotnet build src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

- [ ] **Step 8: Commit**

```bash
git add src/AgenteSuporteGlpi/Program.cs
git commit -m "feat(sistemas): wire system identification into pipeline after save"
```

---

### Task 8: Final verification

**Files:** (none new)

- [ ] **Step 1: Run all tests**

```bash
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj
```

Expected: All tests pass.

- [ ] **Step 2: Run build with TreatWarningsAsErrors**

```bash
dotnet build src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

Expected: No warnings, no errors.

- [ ] **Step 3: Verify git status is clean**

```bash
git status
```

---

## Self-Review

1. **Spec coverage:**
   - `SistemaConfigurado` (Task 1) cobre Nome/Ativo/Aliases/PalavrasChave/Repositorios/Bancos/Observacoes
   - `ConfiguracaoSistemas` (Task 2) carrega de `IConfiguration`
   - `ResultadoIdentificacaoSistema` (Task 3) cobre sistema/confianca/pontuacao/termos/motivo
   - `IdentificadorSistemaPorPalavrasChave` (Task 4) implementa regras deterministicas
   - Tests (Task 6) cobrem aliases, palavras-chave, empate, inativo, normalizacao, chamados reais
   - Pipeline (Task 7) chama identificacao apos salvar
   - Fora do escopo (conforme spec): MAF, SQL Server MCP, busca em repos, relatorio HTML

2. **Placeholder scan:** Nenhum TBD ou TODO.

3. **Type consistency:** `SistemaConfigurado` definido no Task 1, usado em Task 2, 4, 6, 7. `ResultadoIdentificacaoSistema` definido no Task 3, usado em Task 4, 6, 7. `NivelConfianca` definido no Task 3, usado em Task 4, 6, 7.
