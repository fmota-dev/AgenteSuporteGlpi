using Microsoft.Data.Sqlite;

namespace AgenteSuporteGlpi.Banco;

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
