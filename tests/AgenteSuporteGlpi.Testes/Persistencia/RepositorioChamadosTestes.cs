using AgenteSuporteGlpi.Chamados;
using AgenteSuporteGlpi.Banco;
using AgenteSuporteGlpi.Sistemas;
using FluentAssertions;
using Microsoft.Data.Sqlite;

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

    [Fact]
    public async Task Deve_persistir_identificacao_de_sistema()
    {
        await using var banco = BancoTeste.Criar();
        await new InicializadorBanco(banco.ConnectionString).InicializarAsync(CancellationToken.None);
        var repositorio = new RepositorioChamados(banco.ConnectionString);

        var chamado = new DetalhesChamadoColetado(
            31905,
            "Sistema de pesquisas - Criacao de novas perguntas",
            "Descricao do chamado de pesquisas.",
            StatusChamado.Novo,
            "Media",
            null,
            null,
            "Filipe Mota",
            DateTimeOffset.Parse("2026-06-23T09:00:00-03:00"),
            DateTimeOffset.Parse("2026-06-23T10:00:00-03:00"),
            new Uri("https://glpi.local/front/ticket.form.php?id=31905"));

        await repositorio.SalvarChamadoAsync(chamado, "hash-31905", CancellationToken.None);

        var resultado = new ResultadoIdentificacaoSistema
        {
            Sistema = new SistemaConfigurado
            {
                Nome = "Sistema de Pesquisas",
                Ativo = true,
                Aliases = ["pesquisas"],
                PalavrasChave = ["perguntas", "questionarios"],
                Repositorios = [],
                Bancos = []
            },
            Confianca = NivelConfianca.Alta,
            Pontuacao = 30,
            TermosEncontrados = ["pesquisas", "perguntas"],
            Motivo = "Alias 'pesquisas' encontrado no titulo (10pts), palavra-chave 'perguntas' no titulo (5pts)"
        };

        await repositorio.PersistirIdentificacaoAsync(31905, resultado, CancellationToken.None);

        await using var conexao = new SqliteConnection(banco.ConnectionString);
        await conexao.OpenAsync(TestContext.Current.CancellationToken);
        var cmd = conexao.CreateCommand();
        cmd.CommandText = "SELECT Sistema, NivelConfianca, Pontuacao, TermosEncontrados FROM IdentificacoesSistema WHERE NumeroChamado = 31905 ORDER BY Id DESC LIMIT 1";

        await using var reader = await cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        (await reader.ReadAsync(TestContext.Current.CancellationToken)).Should().BeTrue();
        reader.GetString(0).Should().Be("Sistema de Pesquisas");
        reader.GetString(1).Should().Be("Alta");
        reader.GetInt32(2).Should().Be(30);
        reader.GetString(3).Should().Be("pesquisas; perguntas");
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

        public async ValueTask DisposeAsync()
        {
            SqliteConnection.ClearAllPools();

            for (var tentativa = 0; tentativa < 5; tentativa++)
            {
                try
                {
                    if (File.Exists(_caminho))
                    {
                        File.Delete(_caminho);
                    }

                    return;
                }
                catch (IOException) when (tentativa < 4)
                {
                    await Task.Delay(50);
                }
            }
        }
    }
}
