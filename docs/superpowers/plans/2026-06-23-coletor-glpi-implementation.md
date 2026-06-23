# Coletor GLPI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Construir a primeira entrega do Agente de Pre-Analise GLPI: coleta somente leitura de lista + detalhes de chamados GLPI via Playwright .NET, com nucleo testavel e historico em SQLite.

**Architecture:** A solucao separa entrada CLI, regras puras, automacao web e persistencia. Playwright fica isolado em `AgenteSuporteGlpi.ColetaGlpi`; regras e deteccao de mudanca ficam em `AgenteSuporteGlpi.Nucleo`; SQLite fica em `AgenteSuporteGlpi.Persistencia`.

**Tech Stack:** .NET 10, C# 14, Playwright .NET, Microsoft.Data.Sqlite, Microsoft.Extensions.Hosting, Microsoft.Extensions.Configuration.UserSecrets, xUnit, FluentAssertions.

---

## Estrutura De Arquivos

- Create: `AgenteSuporteGlpi.sln`
- Create: `Directory.Build.props`
- Create: `src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj`
- Create: `src/AgenteSuporteGlpi.Console/Program.cs`
- Create: `src/AgenteSuporteGlpi.Console/appsettings.json`
- Create: `src/AgenteSuporteGlpi.Nucleo/AgenteSuporteGlpi.Nucleo.csproj`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/StatusChamado.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/ChamadoColetado.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/DetalhesChamadoColetado.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/FiltroChamados.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/HashConteudoChamado.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/ResultadoMudancaChamado.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/AgenteSuporteGlpi.ColetaGlpi.csproj`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Configuracao/ConfiguracaoGlpi.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Configuracao/ConfiguracaoBrowser.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Contratos/IColetorGlpi.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Playwright/ColetorGlpiPlaywright.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Playwright/SeletoresGlpi.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/AgenteSuporteGlpi.Persistencia.csproj`
- Create: `src/AgenteSuporteGlpi.Persistencia/Banco/ConfiguracaoBanco.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Banco/InicializadorBanco.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Chamados/RepositorioChamados.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Chamados/IRepositorioChamados.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Execucoes/ExecucaoColeta.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Execucoes/EventoExecucao.cs`
- Create: `tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj`
- Create: `tests/AgenteSuporteGlpi.Testes/Nucleo/FiltroChamadosTestes.cs`
- Create: `tests/AgenteSuporteGlpi.Testes/Nucleo/HashConteudoChamadoTestes.cs`
- Create: `tests/AgenteSuporteGlpi.Testes/Nucleo/ResultadoMudancaChamadoTestes.cs`
- Create: `tests/AgenteSuporteGlpi.Testes/Persistencia/RepositorioChamadosTestes.cs`
- Create: `tests/AgenteSuporteGlpi.Testes/ColetaGlpi/ParsingHtmlChamadoTestes.cs`
- Create: `tests/AgenteSuporteGlpi.Testes/Fixtures/chamado-detalhe.html`

## Convencoes Do Plano

- Commits usam Conventional Commits com descricao em pt-BR, exemplo: `feat: adicionar filtro de chamados`.
- Nomes de dominio ficam em pt-BR.
- Ingles fica em nomes de bibliotecas e termos tecnicos inevitaveis.
- Nenhum teste automatizado abre navegador.
- Toda tarefa termina com teste e commit.

### Task 1: Estrutura Inicial Da Solucao

**Files:**
- Create: `AgenteSuporteGlpi.sln`
- Create: `Directory.Build.props`
- Create: `src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj`
- Create: `src/AgenteSuporteGlpi.Nucleo/AgenteSuporteGlpi.Nucleo.csproj`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/AgenteSuporteGlpi.ColetaGlpi.csproj`
- Create: `src/AgenteSuporteGlpi.Persistencia/AgenteSuporteGlpi.Persistencia.csproj`
- Create: `tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj`

- [ ] **Step 1: Criar solution e projetos**

Run:

```powershell
dotnet new sln -n AgenteSuporteGlpi
dotnet new console -n AgenteSuporteGlpi.Console -o src/AgenteSuporteGlpi.Console --framework net10.0
dotnet new classlib -n AgenteSuporteGlpi.Nucleo -o src/AgenteSuporteGlpi.Nucleo --framework net10.0
dotnet new classlib -n AgenteSuporteGlpi.ColetaGlpi -o src/AgenteSuporteGlpi.ColetaGlpi --framework net10.0
dotnet new classlib -n AgenteSuporteGlpi.Persistencia -o src/AgenteSuporteGlpi.Persistencia --framework net10.0
dotnet new xunit -n AgenteSuporteGlpi.Testes -o tests/AgenteSuporteGlpi.Testes --framework net10.0
dotnet sln AgenteSuporteGlpi.sln add src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj
dotnet sln AgenteSuporteGlpi.sln add src/AgenteSuporteGlpi.Nucleo/AgenteSuporteGlpi.Nucleo.csproj
dotnet sln AgenteSuporteGlpi.sln add src/AgenteSuporteGlpi.ColetaGlpi/AgenteSuporteGlpi.ColetaGlpi.csproj
dotnet sln AgenteSuporteGlpi.sln add src/AgenteSuporteGlpi.Persistencia/AgenteSuporteGlpi.Persistencia.csproj
dotnet sln AgenteSuporteGlpi.sln add tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj
```

Expected: projetos criados e adicionados na solution.

- [ ] **Step 2: Adicionar referencias entre projetos**

Run:

```powershell
dotnet add src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj reference src/AgenteSuporteGlpi.Nucleo/AgenteSuporteGlpi.Nucleo.csproj
dotnet add src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj reference src/AgenteSuporteGlpi.ColetaGlpi/AgenteSuporteGlpi.ColetaGlpi.csproj
dotnet add src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj reference src/AgenteSuporteGlpi.Persistencia/AgenteSuporteGlpi.Persistencia.csproj
dotnet add src/AgenteSuporteGlpi.ColetaGlpi/AgenteSuporteGlpi.ColetaGlpi.csproj reference src/AgenteSuporteGlpi.Nucleo/AgenteSuporteGlpi.Nucleo.csproj
dotnet add src/AgenteSuporteGlpi.Persistencia/AgenteSuporteGlpi.Persistencia.csproj reference src/AgenteSuporteGlpi.Nucleo/AgenteSuporteGlpi.Nucleo.csproj
dotnet add tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj reference src/AgenteSuporteGlpi.Nucleo/AgenteSuporteGlpi.Nucleo.csproj
dotnet add tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj reference src/AgenteSuporteGlpi.Persistencia/AgenteSuporteGlpi.Persistencia.csproj
dotnet add tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj reference src/AgenteSuporteGlpi.ColetaGlpi/AgenteSuporteGlpi.ColetaGlpi.csproj
```

Expected: referencias registradas nos `.csproj`.

- [ ] **Step 3: Adicionar pacotes NuGet**

Run:

```powershell
dotnet add src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj package Microsoft.Extensions.Hosting
dotnet add src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj package Microsoft.Extensions.Configuration.UserSecrets
dotnet add src/AgenteSuporteGlpi.ColetaGlpi/AgenteSuporteGlpi.ColetaGlpi.csproj package Microsoft.Playwright
dotnet add src/AgenteSuporteGlpi.Persistencia/AgenteSuporteGlpi.Persistencia.csproj package Microsoft.Data.Sqlite
dotnet add tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj package FluentAssertions
```

Expected: pacotes instalados.

- [ ] **Step 4: Criar `Directory.Build.props`**

Create `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest</AnalysisLevel>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

- [ ] **Step 5: Remover classes padrão vazias**

Delete files if created by template:

```text
src/AgenteSuporteGlpi.Nucleo/Class1.cs
src/AgenteSuporteGlpi.ColetaGlpi/Class1.cs
src/AgenteSuporteGlpi.Persistencia/Class1.cs
tests/AgenteSuporteGlpi.Testes/UnitTest1.cs
```

- [ ] **Step 6: Build inicial**

Run:

```powershell
dotnet build AgenteSuporteGlpi.sln
```

Expected: `Build succeeded`.

- [ ] **Step 7: Commit**

Run:

```powershell
git add AgenteSuporteGlpi.sln Directory.Build.props src tests
git commit -m "chore: criar estrutura inicial da solucao"
```

### Task 2: Modelos E Filtros Do Nucleo

**Files:**
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/StatusChamado.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/ChamadoColetado.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/DetalhesChamadoColetado.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/FiltroChamados.cs`
- Test: `tests/AgenteSuporteGlpi.Testes/Nucleo/FiltroChamadosTestes.cs`

- [ ] **Step 1: Escrever testes de filtro**

Create `tests/AgenteSuporteGlpi.Testes/Nucleo/FiltroChamadosTestes.cs`:

```csharp
using AgenteSuporteGlpi.Nucleo.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Nucleo;

public sealed class FiltroChamadosTestes
{
    [Fact]
    public void Deve_manter_chamados_do_responsavel_com_status_permitido()
    {
        var chamados = new[]
        {
            NovoChamado(1, "Ana", StatusChamado.Novo),
            NovoChamado(2, "Bruno", StatusChamado.Novo),
            NovoChamado(3, "Ana", StatusChamado.EmAtendimento),
            NovoChamado(4, "Ana", StatusChamado.Solucionado)
        };

        var resultado = FiltroChamados.FiltrarElegiveis(chamados, "Ana");

        resultado.Select(chamado => chamado.Numero).Should().Equal(1, 3);
    }

    [Fact]
    public void Deve_tratar_responsavel_sem_diferenciar_maiusculas()
    {
        var chamados = new[]
        {
            NovoChamado(10, "ANA SILVA", StatusChamado.Pendente)
        };

        var resultado = FiltroChamados.FiltrarElegiveis(chamados, "ana silva");

        resultado.Should().ContainSingle().Which.Numero.Should().Be(10);
    }

    private static ChamadoColetado NovoChamado(int numero, string responsavel, StatusChamado status) =>
        new(
            numero,
            $"Chamado {numero}",
            status,
            "Media",
            responsavel,
            DateTimeOffset.Parse("2026-06-23T10:00:00-03:00"),
            new Uri($"https://glpi.local/front/ticket.form.php?id={numero}"));
}
```

- [ ] **Step 2: Rodar teste para falhar**

Run:

```powershell
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter FiltroChamadosTestes
```

Expected: FAIL por tipos `StatusChamado`, `ChamadoColetado` ou `FiltroChamados` inexistentes.

- [ ] **Step 3: Implementar modelos e filtro**

Create `src/AgenteSuporteGlpi.Nucleo/Chamados/StatusChamado.cs`:

```csharp
namespace AgenteSuporteGlpi.Nucleo.Chamados;

public enum StatusChamado
{
    Desconhecido = 0,
    Novo = 1,
    EmAtendimento = 2,
    Pendente = 3,
    Solucionado = 4,
    Fechado = 5
}
```

Create `src/AgenteSuporteGlpi.Nucleo/Chamados/ChamadoColetado.cs`:

```csharp
namespace AgenteSuporteGlpi.Nucleo.Chamados;

public sealed record ChamadoColetado(
    int Numero,
    string Titulo,
    StatusChamado Status,
    string Prioridade,
    string Responsavel,
    DateTimeOffset DataUltimaAtualizacao,
    Uri Link);
```

Create `src/AgenteSuporteGlpi.Nucleo/Chamados/DetalhesChamadoColetado.cs`:

```csharp
namespace AgenteSuporteGlpi.Nucleo.Chamados;

public sealed record DetalhesChamadoColetado(
    int Numero,
    string Titulo,
    string Descricao,
    StatusChamado Status,
    string Prioridade,
    string? Categoria,
    string? Solicitante,
    string Responsavel,
    DateTimeOffset DataAbertura,
    DateTimeOffset DataUltimaAtualizacao,
    Uri Link);
```

Create `src/AgenteSuporteGlpi.Nucleo/Chamados/FiltroChamados.cs`:

```csharp
namespace AgenteSuporteGlpi.Nucleo.Chamados;

public static class FiltroChamados
{
    private static readonly HashSet<StatusChamado> StatusPermitidos =
    [
        StatusChamado.Novo,
        StatusChamado.EmAtendimento,
        StatusChamado.Pendente
    ];

    public static IReadOnlyList<ChamadoColetado> FiltrarElegiveis(
        IEnumerable<ChamadoColetado> chamados,
        string responsavelConfigurado)
    {
        ArgumentNullException.ThrowIfNull(chamados);

        if (string.IsNullOrWhiteSpace(responsavelConfigurado))
        {
            throw new ArgumentException("Responsavel configurado e obrigatorio.", nameof(responsavelConfigurado));
        }

        return chamados
            .Where(chamado => StatusPermitidos.Contains(chamado.Status))
            .Where(chamado => string.Equals(
                chamado.Responsavel.Trim(),
                responsavelConfigurado.Trim(),
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(chamado => chamado.Numero)
            .ToArray();
    }
}
```

- [ ] **Step 4: Rodar teste para passar**

Run:

```powershell
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter FiltroChamadosTestes
```

Expected: PASS.

- [ ] **Step 5: Commit**

Run:

```powershell
git add src/AgenteSuporteGlpi.Nucleo tests/AgenteSuporteGlpi.Testes/Nucleo/FiltroChamadosTestes.cs
git commit -m "feat: adicionar filtro de chamados elegiveis"
```

### Task 3: Hash E Deteccao De Mudanca

**Files:**
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/HashConteudoChamado.cs`
- Create: `src/AgenteSuporteGlpi.Nucleo/Chamados/ResultadoMudancaChamado.cs`
- Test: `tests/AgenteSuporteGlpi.Testes/Nucleo/HashConteudoChamadoTestes.cs`
- Test: `tests/AgenteSuporteGlpi.Testes/Nucleo/ResultadoMudancaChamadoTestes.cs`

- [ ] **Step 1: Escrever testes de hash**

Create `tests/AgenteSuporteGlpi.Testes/Nucleo/HashConteudoChamadoTestes.cs`:

```csharp
using AgenteSuporteGlpi.Nucleo.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Nucleo;

public sealed class HashConteudoChamadoTestes
{
    [Fact]
    public void Deve_gerar_mesmo_hash_para_texto_com_espacos_equivalentes()
    {
        var primeiro = HashConteudoChamado.Calcular("Erro ao salvar   matricula");
        var segundo = HashConteudoChamado.Calcular(" Erro ao salvar matricula ");

        primeiro.Should().Be(segundo);
    }

    [Fact]
    public void Deve_gerar_hash_diferente_quando_conteudo_mudar()
    {
        var primeiro = HashConteudoChamado.Calcular("Erro ao salvar matricula");
        var segundo = HashConteudoChamado.Calcular("Erro ao excluir matricula");

        primeiro.Should().NotBe(segundo);
    }
}
```

- [ ] **Step 2: Escrever testes de mudanca**

Create `tests/AgenteSuporteGlpi.Testes/Nucleo/ResultadoMudancaChamadoTestes.cs`:

```csharp
using AgenteSuporteGlpi.Nucleo.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Nucleo;

public sealed class ResultadoMudancaChamadoTestes
{
    [Fact]
    public void Deve_marcar_como_novo_quando_nao_houver_hash_anterior()
    {
        var resultado = ResultadoMudancaChamado.Avaliar(hashAnterior: null, hashAtual: "abc");

        resultado.EhNovo.Should().BeTrue();
        resultado.FoiAlterado.Should().BeTrue();
    }

    [Fact]
    public void Deve_marcar_sem_alteracao_quando_hash_for_igual()
    {
        var resultado = ResultadoMudancaChamado.Avaliar("abc", "abc");

        resultado.EhNovo.Should().BeFalse();
        resultado.FoiAlterado.Should().BeFalse();
    }

    [Fact]
    public void Deve_marcar_alterado_quando_hash_mudar()
    {
        var resultado = ResultadoMudancaChamado.Avaliar("abc", "def");

        resultado.EhNovo.Should().BeFalse();
        resultado.FoiAlterado.Should().BeTrue();
    }
}
```

- [ ] **Step 3: Rodar testes para falhar**

Run:

```powershell
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter "HashConteudoChamadoTestes|ResultadoMudancaChamadoTestes"
```

Expected: FAIL por tipos inexistentes.

- [ ] **Step 4: Implementar hash e resultado de mudanca**

Create `src/AgenteSuporteGlpi.Nucleo/Chamados/HashConteudoChamado.cs`:

```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AgenteSuporteGlpi.Nucleo.Chamados;

public static partial class HashConteudoChamado
{
    public static string Calcular(string conteudo)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        var normalizado = EspacosDuplicados().Replace(conteudo.Trim(), " ");
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizado));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosDuplicados();
}
```

Create `src/AgenteSuporteGlpi.Nucleo/Chamados/ResultadoMudancaChamado.cs`:

```csharp
namespace AgenteSuporteGlpi.Nucleo.Chamados;

public sealed record ResultadoMudancaChamado(bool EhNovo, bool FoiAlterado)
{
    public static ResultadoMudancaChamado Avaliar(string? hashAnterior, string hashAtual)
    {
        if (string.IsNullOrWhiteSpace(hashAtual))
        {
            throw new ArgumentException("Hash atual e obrigatorio.", nameof(hashAtual));
        }

        if (string.IsNullOrWhiteSpace(hashAnterior))
        {
            return new ResultadoMudancaChamado(EhNovo: true, FoiAlterado: true);
        }

        var foiAlterado = !string.Equals(hashAnterior, hashAtual, StringComparison.Ordinal);
        return new ResultadoMudancaChamado(EhNovo: false, FoiAlterado: foiAlterado);
    }
}
```

- [ ] **Step 5: Rodar testes para passar**

Run:

```powershell
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter "HashConteudoChamadoTestes|ResultadoMudancaChamadoTestes"
```

Expected: PASS.

- [ ] **Step 6: Commit**

Run:

```powershell
git add src/AgenteSuporteGlpi.Nucleo tests/AgenteSuporteGlpi.Testes/Nucleo
git commit -m "feat: detectar mudancas em chamados"
```

### Task 4: Persistencia SQLite

**Files:**
- Create: `src/AgenteSuporteGlpi.Persistencia/Banco/ConfiguracaoBanco.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Banco/InicializadorBanco.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Chamados/IRepositorioChamados.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Chamados/RepositorioChamados.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Execucoes/ExecucaoColeta.cs`
- Create: `src/AgenteSuporteGlpi.Persistencia/Execucoes/EventoExecucao.cs`
- Test: `tests/AgenteSuporteGlpi.Testes/Persistencia/RepositorioChamadosTestes.cs`

- [ ] **Step 1: Escrever teste de persistencia**

Create `tests/AgenteSuporteGlpi.Testes/Persistencia/RepositorioChamadosTestes.cs`:

```csharp
using AgenteSuporteGlpi.Nucleo.Chamados;
using AgenteSuporteGlpi.Persistencia.Banco;
using AgenteSuporteGlpi.Persistencia.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Persistencia;

public sealed class RepositorioChamadosTestes
{
    [Fact]
    public async Task Deve_salvar_chamado_e_retornar_hash_mais_recente()
    {
        await using var banco = BancoTeste.Criar();
        await new InicializadorBanco(banco.ConnectionString).InicializarAsync(CancellationToken.None);
        var repositorio = new RepositorioChamados(banco.ConnectionString);

        var chamado = new DetalhesChamadoColetado(
            123,
            "Erro ao salvar matricula",
            "Mensagem de erro ao salvar matricula do aluno.",
            StatusChamado.Novo,
            "Media",
            "Academico",
            "Maria",
            "Ana",
            DateTimeOffset.Parse("2026-06-23T09:00:00-03:00"),
            DateTimeOffset.Parse("2026-06-23T10:00:00-03:00"),
            new Uri("https://glpi.local/front/ticket.form.php?id=123"));

        await repositorio.SalvarChamadoAsync(chamado, "hash-123", CancellationToken.None);

        var hash = await repositorio.ObterUltimoHashAsync(123, CancellationToken.None);
        hash.Should().Be("hash-123");
    }

    private sealed class BancoTeste : IAsyncDisposable
    {
        private readonly string _caminho;

        private BancoTeste(string caminho)
        {
            _caminho = caminho;
            ConnectionString = $"Data Source={caminho}";
        }

        public string ConnectionString { get; }

        public static BancoTeste Criar()
        {
            var caminho = Path.Combine(Path.GetTempPath(), $"agente-suporte-glpi-{Guid.NewGuid():N}.db");
            return new BancoTeste(caminho);
        }

        public ValueTask DisposeAsync()
        {
            if (File.Exists(_caminho))
            {
                File.Delete(_caminho);
            }

            return ValueTask.CompletedTask;
        }
    }
}
```

- [ ] **Step 2: Rodar teste para falhar**

Run:

```powershell
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter RepositorioChamadosTestes
```

Expected: FAIL por tipos de persistencia inexistentes.

- [ ] **Step 3: Implementar inicializador e repositorio**

Create `src/AgenteSuporteGlpi.Persistencia/Banco/ConfiguracaoBanco.cs`:

```csharp
namespace AgenteSuporteGlpi.Persistencia.Banco;

public sealed class ConfiguracaoBanco
{
    public string ConnectionString { get; init; } = "Data Source=dados/agente-suporte-glpi.db";
}
```

Create `src/AgenteSuporteGlpi.Persistencia/Banco/InicializadorBanco.cs`:

```csharp
using Microsoft.Data.Sqlite;

namespace AgenteSuporteGlpi.Persistencia.Banco;

public sealed class InicializadorBanco(string connectionString)
{
    public async Task InicializarAsync(CancellationToken cancellationToken)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync(cancellationToken);

        var comando = conexao.CreateCommand();
        comando.CommandText = """
            CREATE TABLE IF NOT EXISTS Chamados (
                Numero INTEGER PRIMARY KEY,
                TituloAtual TEXT NOT NULL,
                StatusAtual TEXT NOT NULL,
                PrioridadeAtual TEXT NOT NULL,
                Responsavel TEXT NOT NULL,
                Solicitante TEXT NULL,
                Categoria TEXT NULL,
                DataAbertura TEXT NOT NULL,
                DataUltimaAtualizacao TEXT NOT NULL,
                Link TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ColetasChamado (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NumeroChamado INTEGER NOT NULL,
                DescricaoColetada TEXT NOT NULL,
                HashConteudo TEXT NOT NULL,
                StatusColeta TEXT NOT NULL,
                DataColeta TEXT NOT NULL,
                FOREIGN KEY (NumeroChamado) REFERENCES Chamados(Numero)
            );

            CREATE TABLE IF NOT EXISTS ExecucoesColeta (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Inicio TEXT NOT NULL,
                Fim TEXT NULL,
                Status TEXT NOT NULL,
                QuantidadeEncontrada INTEGER NOT NULL DEFAULT 0,
                QuantidadeColetada INTEGER NOT NULL DEFAULT 0,
                QuantidadeIgnorada INTEGER NOT NULL DEFAULT 0,
                QuantidadeComErro INTEGER NOT NULL DEFAULT 0,
                MensagemErro TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS EventosExecucao (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ExecucaoId INTEGER NULL,
                DataHora TEXT NOT NULL,
                Nivel TEXT NOT NULL,
                Etapa TEXT NOT NULL,
                Mensagem TEXT NOT NULL,
                NumeroChamado INTEGER NULL
            );
            """;

        await comando.ExecuteNonQueryAsync(cancellationToken);
    }
}
```

Create `src/AgenteSuporteGlpi.Persistencia/Chamados/IRepositorioChamados.cs`:

```csharp
using AgenteSuporteGlpi.Nucleo.Chamados;

namespace AgenteSuporteGlpi.Persistencia.Chamados;

public interface IRepositorioChamados
{
    Task<string?> ObterUltimoHashAsync(int numeroChamado, CancellationToken cancellationToken);
    Task SalvarChamadoAsync(DetalhesChamadoColetado chamado, string hashConteudo, CancellationToken cancellationToken);
}
```

Create `src/AgenteSuporteGlpi.Persistencia/Chamados/RepositorioChamados.cs`:

```csharp
using AgenteSuporteGlpi.Nucleo.Chamados;
using Microsoft.Data.Sqlite;

namespace AgenteSuporteGlpi.Persistencia.Chamados;

public sealed class RepositorioChamados(string connectionString) : IRepositorioChamados
{
    public async Task<string?> ObterUltimoHashAsync(int numeroChamado, CancellationToken cancellationToken)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync(cancellationToken);

        var comando = conexao.CreateCommand();
        comando.CommandText = """
            SELECT HashConteudo
            FROM ColetasChamado
            WHERE NumeroChamado = $numeroChamado
            ORDER BY Id DESC
            LIMIT 1
            """;
        comando.Parameters.AddWithValue("$numeroChamado", numeroChamado);

        var resultado = await comando.ExecuteScalarAsync(cancellationToken);
        return resultado as string;
    }

    public async Task SalvarChamadoAsync(DetalhesChamadoColetado chamado, string hashConteudo, CancellationToken cancellationToken)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync(cancellationToken);
        await using var transacao = await conexao.BeginTransactionAsync(cancellationToken);

        var upsert = conexao.CreateCommand();
        upsert.Transaction = (SqliteTransaction)transacao;
        upsert.CommandText = """
            INSERT INTO Chamados (
                Numero, TituloAtual, StatusAtual, PrioridadeAtual, Responsavel, Solicitante,
                Categoria, DataAbertura, DataUltimaAtualizacao, Link)
            VALUES (
                $numero, $titulo, $status, $prioridade, $responsavel, $solicitante,
                $categoria, $dataAbertura, $dataUltimaAtualizacao, $link)
            ON CONFLICT(Numero) DO UPDATE SET
                TituloAtual = excluded.TituloAtual,
                StatusAtual = excluded.StatusAtual,
                PrioridadeAtual = excluded.PrioridadeAtual,
                Responsavel = excluded.Responsavel,
                Solicitante = excluded.Solicitante,
                Categoria = excluded.Categoria,
                DataAbertura = excluded.DataAbertura,
                DataUltimaAtualizacao = excluded.DataUltimaAtualizacao,
                Link = excluded.Link
            """;

        upsert.Parameters.AddWithValue("$numero", chamado.Numero);
        upsert.Parameters.AddWithValue("$titulo", chamado.Titulo);
        upsert.Parameters.AddWithValue("$status", chamado.Status.ToString());
        upsert.Parameters.AddWithValue("$prioridade", chamado.Prioridade);
        upsert.Parameters.AddWithValue("$responsavel", chamado.Responsavel);
        upsert.Parameters.AddWithValue("$solicitante", (object?)chamado.Solicitante ?? DBNull.Value);
        upsert.Parameters.AddWithValue("$categoria", (object?)chamado.Categoria ?? DBNull.Value);
        upsert.Parameters.AddWithValue("$dataAbertura", chamado.DataAbertura.ToString("O"));
        upsert.Parameters.AddWithValue("$dataUltimaAtualizacao", chamado.DataUltimaAtualizacao.ToString("O"));
        upsert.Parameters.AddWithValue("$link", chamado.Link.ToString());
        await upsert.ExecuteNonQueryAsync(cancellationToken);

        var inserirColeta = conexao.CreateCommand();
        inserirColeta.Transaction = (SqliteTransaction)transacao;
        inserirColeta.CommandText = """
            INSERT INTO ColetasChamado (NumeroChamado, DescricaoColetada, HashConteudo, StatusColeta, DataColeta)
            VALUES ($numeroChamado, $descricao, $hash, $statusColeta, $dataColeta)
            """;
        inserirColeta.Parameters.AddWithValue("$numeroChamado", chamado.Numero);
        inserirColeta.Parameters.AddWithValue("$descricao", chamado.Descricao);
        inserirColeta.Parameters.AddWithValue("$hash", hashConteudo);
        inserirColeta.Parameters.AddWithValue("$statusColeta", "Coletado");
        inserirColeta.Parameters.AddWithValue("$dataColeta", DateTimeOffset.UtcNow.ToString("O"));
        await inserirColeta.ExecuteNonQueryAsync(cancellationToken);

        await transacao.CommitAsync(cancellationToken);
    }
}
```

Create `src/AgenteSuporteGlpi.Persistencia/Execucoes/ExecucaoColeta.cs`:

```csharp
namespace AgenteSuporteGlpi.Persistencia.Execucoes;

public sealed record ExecucaoColeta(
    long Id,
    DateTimeOffset Inicio,
    DateTimeOffset? Fim,
    string Status,
    int QuantidadeEncontrada,
    int QuantidadeColetada,
    int QuantidadeIgnorada,
    int QuantidadeComErro,
    string? MensagemErro);
```

Create `src/AgenteSuporteGlpi.Persistencia/Execucoes/EventoExecucao.cs`:

```csharp
namespace AgenteSuporteGlpi.Persistencia.Execucoes;

public sealed record EventoExecucao(
    long Id,
    long? ExecucaoId,
    DateTimeOffset DataHora,
    string Nivel,
    string Etapa,
    string Mensagem,
    int? NumeroChamado);
```

- [ ] **Step 4: Rodar teste para passar**

Run:

```powershell
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter RepositorioChamadosTestes
```

Expected: PASS.

- [ ] **Step 5: Commit**

Run:

```powershell
git add src/AgenteSuporteGlpi.Persistencia tests/AgenteSuporteGlpi.Testes/Persistencia
git commit -m "feat: persistir historico de chamados em sqlite"
```

### Task 5: Contrato Do Coletor E Parsing Sem Browser

**Files:**
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Contratos/IColetorGlpi.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Playwright/SeletoresGlpi.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Playwright/ParserDetalhesChamado.cs`
- Create: `tests/AgenteSuporteGlpi.Testes/Fixtures/chamado-detalhe.html`
- Test: `tests/AgenteSuporteGlpi.Testes/ColetaGlpi/ParsingHtmlChamadoTestes.cs`

- [ ] **Step 1: Escrever fixture HTML**

Create `tests/AgenteSuporteGlpi.Testes/Fixtures/chamado-detalhe.html`:

```html
<main id="ticket-content">
  <h1>Erro ao salvar matricula</h1>
  <dl>
    <dt>Numero</dt><dd>123</dd>
    <dt>Status</dt><dd>Novo</dd>
    <dt>Prioridade</dt><dd>Media</dd>
    <dt>Categoria</dt><dd>Academico</dd>
    <dt>Solicitante</dt><dd>Maria</dd>
    <dt>Responsavel</dt><dd>Ana</dd>
    <dt>Abertura</dt><dd>2026-06-23T09:00:00-03:00</dd>
    <dt>Ultima atualizacao</dt><dd>2026-06-23T10:00:00-03:00</dd>
  </dl>
  <section id="descricao">Mensagem de erro ao salvar matricula do aluno.</section>
</main>
```

- [ ] **Step 2: Escrever teste de parsing**

Create `tests/AgenteSuporteGlpi.Testes/ColetaGlpi/ParsingHtmlChamadoTestes.cs`:

```csharp
using AgenteSuporteGlpi.ColetaGlpi.Playwright;
using AgenteSuporteGlpi.Nucleo.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.ColetaGlpi;

public sealed class ParsingHtmlChamadoTestes
{
    [Fact]
    public async Task Deve_converter_html_de_detalhe_em_chamado()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-detalhe.html");

        var chamado = ParserDetalhesChamado.Converter(
            html,
            new Uri("https://glpi.local/front/ticket.form.php?id=123"));

        chamado.Numero.Should().Be(123);
        chamado.Titulo.Should().Be("Erro ao salvar matricula");
        chamado.Status.Should().Be(StatusChamado.Novo);
        chamado.Descricao.Should().Be("Mensagem de erro ao salvar matricula do aluno.");
        chamado.Responsavel.Should().Be("Ana");
    }
}
```

- [ ] **Step 3: Rodar teste para falhar**

Run:

```powershell
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter ParsingHtmlChamadoTestes
```

Expected: FAIL por `ParserDetalhesChamado` inexistente.

- [ ] **Step 4: Implementar contrato e parser**

Create `src/AgenteSuporteGlpi.ColetaGlpi/Contratos/IColetorGlpi.cs`:

```csharp
using AgenteSuporteGlpi.Nucleo.Chamados;

namespace AgenteSuporteGlpi.ColetaGlpi.Contratos;

public interface IColetorGlpi
{
    Task<IReadOnlyList<ChamadoColetado>> ColetarListaAsync(CancellationToken cancellationToken);
    Task<DetalhesChamadoColetado> ColetarDetalhesAsync(ChamadoColetado chamado, CancellationToken cancellationToken);
}
```

Create `src/AgenteSuporteGlpi.ColetaGlpi/Playwright/SeletoresGlpi.cs`:

```csharp
namespace AgenteSuporteGlpi.ColetaGlpi.Playwright;

public sealed class SeletoresGlpi
{
    public string CampoUsuario { get; init; } = "input[name='login_name']";
    public string CampoSenha { get; init; } = "input[name='login_password']";
    public string BotaoEntrar { get; init; } = "button[type='submit']";
    public string LinhaChamado { get; init; } = "table tbody tr";
    public string ConteudoChamado { get; init; } = "#ticket-content";
}
```

Create `src/AgenteSuporteGlpi.ColetaGlpi/Playwright/ParserDetalhesChamado.cs`:

```csharp
using System.Text.RegularExpressions;
using AgenteSuporteGlpi.Nucleo.Chamados;

namespace AgenteSuporteGlpi.ColetaGlpi.Playwright;

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
        Limpar(MatchObrigatorio(html, $"<[^>]+id=\\\"{id}\\\"[^>]*>(.*?)</[^>]+>").Groups[1].Value);

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
```

Modify `tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj` to copy fixtures by adding this `ItemGroup` before `</Project>`:

```xml
<ItemGroup>
  <None Include="Fixtures\*.html" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

- [ ] **Step 5: Rodar teste para passar**

Run:

```powershell
dotnet test tests/AgenteSuporteGlpi.Testes/AgenteSuporteGlpi.Testes.csproj --filter ParsingHtmlChamadoTestes
```

Expected: PASS.

- [ ] **Step 6: Commit**

Run:

```powershell
git add src/AgenteSuporteGlpi.ColetaGlpi tests/AgenteSuporteGlpi.Testes
git commit -m "feat: adicionar contrato e parsing de chamados glpi"
```

### Task 6: Automacao Playwright Somente Leitura

**Files:**
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Configuracao/ConfiguracaoGlpi.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Configuracao/ConfiguracaoBrowser.cs`
- Create: `src/AgenteSuporteGlpi.ColetaGlpi/Playwright/ColetorGlpiPlaywright.cs`

- [ ] **Step 1: Criar configuracoes**

Create `src/AgenteSuporteGlpi.ColetaGlpi/Configuracao/ConfiguracaoGlpi.cs`:

```csharp
namespace AgenteSuporteGlpi.ColetaGlpi.Configuracao;

public sealed class ConfiguracaoGlpi
{
    public required Uri UrlBase { get; init; }
    public required string UsuarioLogin { get; init; }
    public required string SenhaLogin { get; init; }
    public required string Responsavel { get; init; }
    public int LimiteChamadosPorExecucao { get; init; } = 5;
}
```

Create `src/AgenteSuporteGlpi.ColetaGlpi/Configuracao/ConfiguracaoBrowser.cs`:

```csharp
namespace AgenteSuporteGlpi.ColetaGlpi.Configuracao;

public sealed class ConfiguracaoBrowser
{
    public bool Headless { get; init; } = true;
    public int TimeoutMilissegundos { get; init; } = 30_000;
}
```

- [ ] **Step 2: Implementar esqueleto Playwright com guardrails**

Create `src/AgenteSuporteGlpi.ColetaGlpi/Playwright/ColetorGlpiPlaywright.cs`:

```csharp
using AgenteSuporteGlpi.ColetaGlpi.Configuracao;
using AgenteSuporteGlpi.ColetaGlpi.Contratos;
using AgenteSuporteGlpi.Nucleo.Chamados;
using Microsoft.Playwright;

namespace AgenteSuporteGlpi.ColetaGlpi.Playwright;

public sealed class ColetorGlpiPlaywright(
    ConfiguracaoGlpi configuracaoGlpi,
    ConfiguracaoBrowser configuracaoBrowser,
    SeletoresGlpi seletores) : IColetorGlpi
{
    public async Task<IReadOnlyList<ChamadoColetado>> ColetarListaAsync(CancellationToken cancellationToken)
    {
        await using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = configuracaoBrowser.Headless,
            Timeout = configuracaoBrowser.TimeoutMilissegundos
        });

        var pagina = await AbrirPaginaAutenticadaAsync(browser, cancellationToken);
        await pagina.GotoAsync(new Uri(configuracaoGlpi.UrlBase, "/front/ticket.php").ToString());

        var linhas = await pagina.Locator(seletores.LinhaChamado).AllAsync();
        var chamados = new List<ChamadoColetado>();

        foreach (var linha in linhas.Take(configuracaoGlpi.LimiteChamadosPorExecucao))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var texto = await linha.InnerTextAsync();
            var chamado = ConverterLinha(texto, configuracaoGlpi.UrlBase);
            chamados.Add(chamado);
        }

        return chamados;
    }

    public async Task<DetalhesChamadoColetado> ColetarDetalhesAsync(ChamadoColetado chamado, CancellationToken cancellationToken)
    {
        await using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = configuracaoBrowser.Headless,
            Timeout = configuracaoBrowser.TimeoutMilissegundos
        });

        var pagina = await AbrirPaginaAutenticadaAsync(browser, cancellationToken);
        await pagina.GotoAsync(chamado.Link.ToString());
        var html = await pagina.Locator(seletores.ConteudoChamado).InnerHTMLAsync();

        return ParserDetalhesChamado.Converter(html, chamado.Link);
    }

    private async Task<IPage> AbrirPaginaAutenticadaAsync(IBrowser browser, CancellationToken cancellationToken)
    {
        var pagina = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = configuracaoGlpi.UrlBase.ToString()
        });

        pagina.SetDefaultTimeout(configuracaoBrowser.TimeoutMilissegundos);
        await pagina.GotoAsync(configuracaoGlpi.UrlBase.ToString());
        await pagina.Locator(seletores.CampoUsuario).FillAsync(configuracaoGlpi.UsuarioLogin);
        await pagina.Locator(seletores.CampoSenha).FillAsync(configuracaoGlpi.SenhaLogin);
        await pagina.Locator(seletores.BotaoEntrar).ClickAsync();

        await BloquearFluxosInesperadosAsync(pagina, cancellationToken);
        return pagina;
    }

    private static async Task BloquearFluxosInesperadosAsync(IPage pagina, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var texto = await pagina.Locator("body").InnerTextAsync();

        if (texto.Contains("captcha", StringComparison.OrdinalIgnoreCase) ||
            texto.Contains("alterar senha", StringComparison.OrdinalIgnoreCase) ||
            texto.Contains("mfa", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("GLPI solicitou etapa interativa inesperada. Execucao abortada em modo seguro.");
        }
    }

    private static ChamadoColetado ConverterLinha(string textoLinha, Uri urlBase)
    {
        var partes = textoLinha.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (partes.Length < 5)
        {
            throw new InvalidOperationException("Linha de chamado GLPI nao contem colunas esperadas.");
        }

        var numero = int.Parse(partes[0]);
        return new ChamadoColetado(
            numero,
            partes[1],
            ConverterStatus(partes[2]),
            partes[3],
            partes[4],
            DateTimeOffset.UtcNow,
            new Uri(urlBase, $"/front/ticket.form.php?id={numero}"));
    }

    private static StatusChamado ConverterStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "novo" => StatusChamado.Novo,
        "em atendimento" => StatusChamado.EmAtendimento,
        "pendente" => StatusChamado.Pendente,
        "solucionado" => StatusChamado.Solucionado,
        "fechado" => StatusChamado.Fechado,
        _ => StatusChamado.Desconhecido
    };
}
```

- [ ] **Step 3: Build para validar API Playwright**

Run:

```powershell
dotnet build AgenteSuporteGlpi.sln
```

Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

Run:

```powershell
git add src/AgenteSuporteGlpi.ColetaGlpi
git commit -m "feat: adicionar coletor glpi com playwright"
```

### Task 7: Console, Configuracao E User Secrets

**Files:**
- Create: `src/AgenteSuporteGlpi.Console/appsettings.json`
- Modify: `src/AgenteSuporteGlpi.Console/Program.cs`

- [ ] **Step 1: Criar appsettings sem segredos**

Create `src/AgenteSuporteGlpi.Console/appsettings.json`:

```json
{
  "Glpi": {
    "UrlBase": "https://glpi.exemplo.local/",
    "Responsavel": "Nome Do Responsavel",
    "LimiteChamadosPorExecucao": 5
  },
  "Browser": {
    "Headless": true,
    "TimeoutMilissegundos": 30000
  },
  "Banco": {
    "ConnectionString": "Data Source=dados/agente-suporte-glpi.db"
  }
}
```

- [ ] **Step 2: Implementar Program.cs**

Modify `src/AgenteSuporteGlpi.Console/Program.cs`:

```csharp
using AgenteSuporteGlpi.ColetaGlpi.Configuracao;
using AgenteSuporteGlpi.ColetaGlpi.Contratos;
using AgenteSuporteGlpi.ColetaGlpi.Playwright;
using AgenteSuporteGlpi.Nucleo.Chamados;
using AgenteSuporteGlpi.Persistencia.Banco;
using AgenteSuporteGlpi.Persistencia.Chamados;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

var configuracaoGlpi = builder.Configuration.GetSection("Glpi").Get<ConfiguracaoGlpi>()
    ?? throw new InvalidOperationException("Configuracao Glpi ausente.");
var configuracaoBrowser = builder.Configuration.GetSection("Browser").Get<ConfiguracaoBrowser>() ?? new ConfiguracaoBrowser();
var configuracaoBanco = builder.Configuration.GetSection("Banco").Get<ConfiguracaoBanco>() ?? new ConfiguracaoBanco();

builder.Services.AddSingleton(configuracaoGlpi);
builder.Services.AddSingleton(configuracaoBrowser);
builder.Services.AddSingleton(configuracaoBanco);
builder.Services.AddSingleton<SeletoresGlpi>();
builder.Services.AddSingleton<IColetorGlpi, ColetorGlpiPlaywright>();
builder.Services.AddSingleton<IRepositorioChamados>(_ => new RepositorioChamados(configuracaoBanco.ConnectionString));
builder.Services.AddSingleton(_ => new InicializadorBanco(configuracaoBanco.ConnectionString));

using var host = builder.Build();
var cancellationToken = CancellationToken.None;

var inicializador = host.Services.GetRequiredService<InicializadorBanco>();
await inicializador.InicializarAsync(cancellationToken);

var coletor = host.Services.GetRequiredService<IColetorGlpi>();
var repositorio = host.Services.GetRequiredService<IRepositorioChamados>();

var chamados = await coletor.ColetarListaAsync(cancellationToken);
var elegiveis = FiltroChamados.FiltrarElegiveis(chamados, configuracaoGlpi.Responsavel);

var coletados = 0;
var ignorados = 0;

foreach (var chamado in elegiveis)
{
    var detalhes = await coletor.ColetarDetalhesAsync(chamado, cancellationToken);
    var hashAtual = HashConteudoChamado.Calcular($"{detalhes.Titulo}\n{detalhes.Descricao}\n{detalhes.Status}\n{detalhes.DataUltimaAtualizacao:O}");
    var hashAnterior = await repositorio.ObterUltimoHashAsync(detalhes.Numero, cancellationToken);
    var mudanca = ResultadoMudancaChamado.Avaliar(hashAnterior, hashAtual);

    if (!mudanca.FoiAlterado)
    {
        ignorados++;
        continue;
    }

    await repositorio.SalvarChamadoAsync(detalhes, hashAtual, cancellationToken);
    coletados++;
}

Console.WriteLine($"Chamados encontrados: {chamados.Count}");
Console.WriteLine($"Chamados elegiveis: {elegiveis.Count}");
Console.WriteLine($"Chamados coletados: {coletados}");
Console.WriteLine($"Chamados ignorados: {ignorados}");
```

- [ ] **Step 3: Inicializar User Secrets**

Run:

```powershell
dotnet user-secrets init --project src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj
dotnet user-secrets set "Glpi:UsuarioLogin" "usuario.local" --project src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj
dotnet user-secrets set "Glpi:SenhaLogin" "senha-local" --project src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj
```

Expected: secrets gravados fora do repositorio. Antes de usar GLPI real, substituir valores de exemplo pelos dados reais localmente.

- [ ] **Step 4: Build**

Run:

```powershell
dotnet build AgenteSuporteGlpi.sln
```

Expected: `Build succeeded`.

- [ ] **Step 5: Commit**

Run:

```powershell
git add src/AgenteSuporteGlpi.Console
git commit -m "feat: configurar execucao do coletor glpi"
```

### Task 8: Validacao Final E Documentacao De Uso

**Files:**
- Create: `docs/uso-local-coletor-glpi.md`

- [ ] **Step 1: Criar documentacao de uso local**

Create `docs/uso-local-coletor-glpi.md`:

```markdown
# Uso Local Do Coletor GLPI

## Objetivo

Executar coleta somente leitura de chamados GLPI atribuídos ao responsável configurado.

## Configuração Não Sensível

Arquivo: `src/AgenteSuporteGlpi.Console/appsettings.json`

- `Glpi:UrlBase`
- `Glpi:Responsavel`
- `Glpi:LimiteChamadosPorExecucao`
- `Browser:Headless`
- `Browser:TimeoutMilissegundos`
- `Banco:ConnectionString`

## Configuração Privada Local

Use User Secrets no projeto Console:

```powershell
dotnet user-secrets init --project src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj
dotnet user-secrets set "Glpi:UsuarioLogin" "seu-usuario" --project src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj
dotnet user-secrets set "Glpi:SenhaLogin" "sua-senha" --project src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj
```

## Execução

```powershell
dotnet run --project src/AgenteSuporteGlpi.Console/AgenteSuporteGlpi.Console.csproj
```

## Segurança

O coletor não deve comentar, salvar, alterar status, prioridade, categoria ou responsável. Se o GLPI exigir captcha, MFA, troca de senha ou confirmação inesperada, a execução deve abortar.
```

- [ ] **Step 2: Rodar suite completa**

Run:

```powershell
dotnet test AgenteSuporteGlpi.sln
```

Expected: todos os testes PASS.

- [ ] **Step 3: Build release**

Run:

```powershell
dotnet build AgenteSuporteGlpi.sln -c Release
```

Expected: `Build succeeded`.

- [ ] **Step 4: Conferir status git**

Run:

```powershell
git status --short
```

Expected: apenas arquivos desta tarefa como modificados antes do commit.

- [ ] **Step 5: Commit**

Run:

```powershell
git add docs/uso-local-coletor-glpi.md
git commit -m "docs: documentar uso local do coletor glpi"
```

## Revisao Do Plano

### Cobertura Da Spec

- Arquitetura Console/Nucleo/ColetaGlpi/Persistencia/Testes: Tasks 1, 2, 4, 5, 6 e 7.
- Coleta GLPI via Playwright somente leitura: Tasks 5 e 6.
- Coleta lista + detalhes: Tasks 5, 6 e 7.
- SQLite desde o inicio: Task 4.
- Detectar chamado novo/alterado: Task 3 e Task 7.
- Configuracao e User Secrets: Task 7.
- Testes sem abrir navegador: Tasks 2, 3, 4 e 5.
- Documentacao de uso local: Task 8.

### Placeholder Scan

Plano nao usa marcadores de lacuna nem passos sem conteudo executavel. Valores como `https://glpi.exemplo.local/`, `usuario.local` e `senha-local` sao exemplos explicitos para configuracao local, nao pendencias de implementacao.

### Consistencia De Tipos

Tipos usados nos testes sao definidos antes ou na mesma tarefa: `ChamadoColetado`, `DetalhesChamadoColetado`, `StatusChamado`, `FiltroChamados`, `HashConteudoChamado`, `ResultadoMudancaChamado`, `InicializadorBanco`, `RepositorioChamados`, `IColetorGlpi` e `ColetorGlpiPlaywright`.
