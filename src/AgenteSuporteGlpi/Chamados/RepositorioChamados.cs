using AgenteSuporteGlpi.IA;
using AgenteSuporteGlpi.Sistemas;
using Microsoft.Data.Sqlite;

namespace AgenteSuporteGlpi.Chamados;

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

    public async Task<IReadOnlyList<DetalhesChamadoColetado>> ObterChamadosNaoAnalisadosAsync(CancellationToken cancellationToken)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync(cancellationToken);

        var comando = conexao.CreateCommand();
        comando.CommandText = """
            SELECT Numero, TituloAtual, DescricaoColetada, StatusAtual, PrioridadeAtual,
                   Responsavel, Solicitante, Categoria, DataAbertura, DataUltimaAtualizacao, Link
            FROM Chamados
            INNER JOIN (
                SELECT NumeroChamado, DescricaoColetada
                FROM ColetasChamado
                WHERE Id IN (
                    SELECT MAX(Id)
                    FROM ColetasChamado
                    GROUP BY NumeroChamado
                )
            ) UltimaColeta ON Chamados.Numero = UltimaColeta.NumeroChamado
            WHERE AnalisadoPorIa = 0
            ORDER BY Chamados.Numero
            """;

        var chamados = new List<DetalhesChamadoColetado>();

        await using var reader = await comando.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var statusStr = reader.GetString(3);
            Enum.TryParse<StatusChamado>(statusStr, out var status);

            var dataAbertura = DateTimeOffset.Parse(reader.GetString(8));
            var dataUltima = DateTimeOffset.Parse(reader.GetString(9));

            chamados.Add(new DetalhesChamadoColetado(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                status,
                reader.GetString(4),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetString(5),
                dataAbertura,
                dataUltima,
                new Uri(reader.GetString(10))
            ));
        }

        return chamados;
    }

    public async Task SalvarAnaliseIaAsync(int numeroChamado, ResultadoAnaliseIa analise, CancellationToken cancellationToken)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync(cancellationToken);

        var comando = conexao.CreateCommand();
        comando.CommandText = """
            INSERT INTO AnalisesIa (NumeroChamado, ResumoTecnico, PerguntasSolicitante, ProximosPassos, DataAnalise)
            VALUES ($numeroChamado, $resumo, $perguntas, $proximosPassos, $dataAnalise)
            """;
        comando.Parameters.AddWithValue("$numeroChamado", numeroChamado);
        comando.Parameters.AddWithValue("$resumo", analise.ResumoTecnico);
        comando.Parameters.AddWithValue("$perguntas", string.Join("|||", analise.PerguntasSolicitante));
        comando.Parameters.AddWithValue("$proximosPassos", string.Join("|||", analise.ProximosPassos));
        comando.Parameters.AddWithValue("$dataAnalise", DateTimeOffset.UtcNow.ToString("O"));
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarcarAnalisadoPorIaAsync(int numeroChamado, CancellationToken cancellationToken)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync(cancellationToken);

        var comando = conexao.CreateCommand();
        comando.CommandText = """
            UPDATE Chamados SET AnalisadoPorIa = 1 WHERE Numero = $numero
            """;
        comando.Parameters.AddWithValue("$numero", numeroChamado);
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task PersistirIdentificacaoAsync(int numeroChamado, ResultadoIdentificacaoSistema resultado, CancellationToken cancellationToken)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync(cancellationToken);

        var comando = conexao.CreateCommand();
        comando.CommandText = """
            INSERT INTO IdentificacoesSistema (NumeroChamado, Sistema, NivelConfianca, Pontuacao, TermosEncontrados, Motivo, DataIdentificacao)
            VALUES ($numeroChamado, $sistema, $nivelConfianca, $pontuacao, $termos, $motivo, $dataIdentificacao)
            """;
        comando.Parameters.AddWithValue("$numeroChamado", numeroChamado);
        comando.Parameters.AddWithValue("$sistema", resultado.Sistema?.Nome ?? "NaoIdentificado");
        comando.Parameters.AddWithValue("$nivelConfianca", resultado.Confianca.ToString());
        comando.Parameters.AddWithValue("$pontuacao", resultado.Pontuacao);
        comando.Parameters.AddWithValue("$termos", string.Join("; ", resultado.TermosEncontrados));
        comando.Parameters.AddWithValue("$motivo", resultado.Motivo);
        comando.Parameters.AddWithValue("$dataIdentificacao", DateTimeOffset.UtcNow.ToString("O"));
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }
}
