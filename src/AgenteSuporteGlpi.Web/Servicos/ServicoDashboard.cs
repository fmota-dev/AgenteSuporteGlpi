using Microsoft.Data.Sqlite;
using AgenteSuporteGlpi.Web.Modelos;

namespace AgenteSuporteGlpi.Web.Servicos;

public sealed class ServicoDashboard(string connectionString)
{
    public async Task<ResumoDashboard> ObterResumoAsync()
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync();

        var resumo = new ResumoDashboard();
        var cmd = conexao.CreateCommand();

        cmd.CommandText = "SELECT COUNT(*) FROM Chamados";
        resumo = resumo with { TotalChamados = Convert.ToInt32(await cmd.ExecuteScalarAsync()) };

        cmd.CommandText = "SELECT COUNT(*) FROM Chamados WHERE AnalisadoPorIa = 1";
        resumo = resumo with { TotalAnalisados = Convert.ToInt32(await cmd.ExecuteScalarAsync()) };

        cmd.CommandText = "SELECT COUNT(DISTINCT Sistema) FROM IdentificacoesSistema WHERE Sistema IS NOT NULL AND Sistema != ''";
        resumo = resumo with { TotalSistemasIdentificados = Convert.ToInt32(await cmd.ExecuteScalarAsync()) };

        cmd.CommandText = "SELECT COUNT(*) FROM IdentificacoesSistema WHERE NivelConfianca = 'Alta'";
        resumo = resumo with { AltaConfianca = Convert.ToInt32(await cmd.ExecuteScalarAsync()) };

        cmd.CommandText = """
            SELECT i.Sistema, COUNT(*) AS Quantidade
            FROM IdentificacoesSistema i
            INNER JOIN (SELECT NumeroChamado, MAX(Id) AS UltimaId FROM IdentificacoesSistema GROUP BY NumeroChamado) ultima
                ON i.Id = ultima.UltimaId
            WHERE i.Sistema IS NOT NULL AND i.Sistema != ''
            GROUP BY i.Sistema ORDER BY Quantidade DESC
            """;
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                resumo.ChamadosPorSistema.Add(new ChamadoPorSistema { Sistema = reader.GetString(0), Quantidade = reader.GetInt32(1) });
        }

        cmd.CommandText = """
            SELECT NivelConfianca, COUNT(*) AS Quantidade
            FROM IdentificacoesSistema
            INNER JOIN (SELECT NumeroChamado, MAX(Id) AS UltimaId FROM IdentificacoesSistema GROUP BY NumeroChamado) ultima
                ON IdentificacoesSistema.Id = ultima.UltimaId
            GROUP BY NivelConfianca
            ORDER BY CASE NivelConfianca WHEN 'Alta' THEN 1 WHEN 'Media' THEN 2 WHEN 'Baixa' THEN 3 ELSE 4 END
            """;
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                resumo.ConfiancaPorNivel.Add(new ConfiancaPorNivel { Nivel = reader.GetString(0), Quantidade = reader.GetInt32(1) });
        }

        cmd.CommandText = """
            SELECT c.Numero, c.TituloAtual, c.StatusAtual, COALESCE(i.Sistema, '-'),
                   COALESCE(i.NivelConfianca, '-'), c.DataUltimaAtualizacao, c.Link
            FROM Chamados c
            LEFT JOIN (SELECT NumeroChamado, Sistema, NivelConfianca FROM IdentificacoesSistema
                       WHERE Id IN (SELECT MAX(Id) FROM IdentificacoesSistema GROUP BY NumeroChamado)) i
                ON c.Numero = i.NumeroChamado
            ORDER BY c.DataUltimaAtualizacao DESC LIMIT 20
            """;
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                resumo.ChamadosRecentes.Add(new ChamadoRecente
                {
                    Numero = reader.GetInt32(0), Titulo = reader.GetString(1), Status = reader.GetString(2),
                    Sistema = reader.GetString(3), Confianca = reader.GetString(4), Data = reader.GetString(5), Link = reader.GetString(6)
                });
        }

        return resumo;
    }

    public async Task<List<ChamadoDetalheViewModel>> ListarChamadosAsync(string? filtroSistema = null)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync();

        var lista = new List<ChamadoDetalheViewModel>();
        var cmd = conexao.CreateCommand();
        var sql = """
            SELECT c.Numero, c.TituloAtual, c.StatusAtual, c.PrioridadeAtual, c.Responsavel,
                   c.Solicitante, c.Categoria, c.DataAbertura, c.DataUltimaAtualizacao, c.Link,
                   COALESCE(i.Sistema,'-'), COALESCE(i.NivelConfianca,'-'), COALESCE(i.Pontuacao,0),
                   COALESCE(i.TermosEncontrados,'-'), a.ResumoTecnico, a.PerguntasSolicitante, a.ProximosPassos,
                   (SELECT COUNT(*) FROM ColetasChamado WHERE NumeroChamado=c.Numero) AS TotalColetas,
                   (SELECT DataColeta FROM ColetasChamado WHERE NumeroChamado=c.Numero ORDER BY Id DESC LIMIT 1) AS UltimaColeta
            FROM Chamados c
            LEFT JOIN (SELECT NumeroChamado, Sistema, NivelConfianca, Pontuacao, TermosEncontrados FROM IdentificacoesSistema
                       WHERE Id IN (SELECT MAX(Id) FROM IdentificacoesSistema GROUP BY NumeroChamado)) i
                ON c.Numero=i.NumeroChamado
            LEFT JOIN (SELECT NumeroChamado, ResumoTecnico, PerguntasSolicitante, ProximosPassos FROM AnalisesIa
                       WHERE Id IN (SELECT MAX(Id) FROM AnalisesIa GROUP BY NumeroChamado)) a
                ON c.Numero=a.NumeroChamado
            """;

        if (!string.IsNullOrWhiteSpace(filtroSistema))
        {
            sql += " WHERE i.Sistema = $sistema";
            cmd.Parameters.AddWithValue("$sistema", filtroSistema);
        }

        sql += " ORDER BY c.DataUltimaAtualizacao DESC LIMIT 100";
        cmd.CommandText = sql;

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new ChamadoDetalheViewModel
            {
                Numero = reader.GetInt32(0), Titulo = reader.GetString(1), Status = reader.GetString(2),
                Prioridade = reader.GetString(3), Responsavel = reader.GetString(4),
                Solicitante = reader.IsDBNull(5) ? null : reader.GetString(5),
                Categoria = reader.IsDBNull(6) ? null : reader.GetString(6),
                DataAbertura = reader.GetString(7), DataUltimaAtualizacao = reader.GetString(8), Link = reader.GetString(9),
                Sistema = reader.GetString(10), Confianca = reader.GetString(11), Pontuacao = reader.GetInt32(12),
                TermosEncontrados = reader.GetString(13),
                ResumoTecnicoIa = reader.IsDBNull(14) ? null : reader.GetString(14),
                PerguntasIa = reader.IsDBNull(15) ? null : reader.GetString(15),
                ProximosPassosIa = reader.IsDBNull(16) ? null : reader.GetString(16),
                TotalColetas = reader.GetInt32(17),
                UltimaColeta = reader.IsDBNull(18) ? null : reader.GetString(18)
            });
        }

        return lista;
    }

    public async Task<List<AnaliseIaViewModel>> ListarAnalisesAsync()
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync();

        var lista = new List<AnaliseIaViewModel>();
        var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT a.NumeroChamado, c.TituloAtual, COALESCE(i.Sistema,'-'),
                   a.ResumoTecnico, a.PerguntasSolicitante, a.ProximosPassos, a.DataAnalise, c.Link
            FROM AnalisesIa a INNER JOIN Chamados c ON a.NumeroChamado=c.Numero
            LEFT JOIN (SELECT NumeroChamado, Sistema FROM IdentificacoesSistema
                       WHERE Id IN (SELECT MAX(Id) FROM IdentificacoesSistema GROUP BY NumeroChamado)) i
                ON a.NumeroChamado=i.NumeroChamado
            ORDER BY a.DataAnalise DESC LIMIT 100
            """;

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new AnaliseIaViewModel
            {
                NumeroChamado = reader.GetInt32(0), TituloChamado = reader.GetString(1), Sistema = reader.GetString(2),
                ResumoTecnico = reader.GetString(3), PerguntasSolicitante = reader.GetString(4),
                ProximosPassos = reader.GetString(5), DataAnalise = reader.GetString(6), Link = reader.GetString(7)
            });
        }

        return lista;
    }

    public async Task<ChamadoDetalheViewModel?> ObterChamadoAsync(int numero)
    {
        var lista = await ListarChamadosAsync();
        return lista.FirstOrDefault(c => c.Numero == numero);
    }

    public async Task<List<string>> ListarSistemasAsync()
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.OpenAsync();

        var cmd = conexao.CreateCommand();
        cmd.CommandText = """
            SELECT DISTINCT i.Sistema FROM IdentificacoesSistema i
            INNER JOIN (SELECT NumeroChamado, MAX(Id) AS UltimaId FROM IdentificacoesSistema GROUP BY NumeroChamado) ultima
                ON i.Id = ultima.UltimaId
            WHERE i.Sistema IS NOT NULL AND i.Sistema != '' ORDER BY i.Sistema
            """;

        var sistemas = new List<string>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            sistemas.Add(reader.GetString(0));

        return sistemas;
    }
}
