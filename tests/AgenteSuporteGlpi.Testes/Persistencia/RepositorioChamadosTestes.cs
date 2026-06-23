using AgenteSuporteGlpi.Chamados;
using AgenteSuporteGlpi.Banco;
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
