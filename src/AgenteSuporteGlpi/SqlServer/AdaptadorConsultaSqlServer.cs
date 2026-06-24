using AgenteSuporteGlpi.Contratos;
using ConsultorSQLServer.Services;

namespace AgenteSuporteGlpi.SqlServer;

public sealed class AdaptadorConsultaSqlServer(ConsultorSqlServerService service) : IConsultaSqlServerSomenteLeitura
{
    public Task<string> ListarConexoesAsync(CancellationToken cancellationToken)
        => service.ListarConexoesConfiguradasAsync();

    public Task<string> ListarBancosAsync(string alias, CancellationToken cancellationToken)
        => service.ListarBancosAsync(alias);

    public Task<string> MapearBancoAsync(string alias, string banco, CancellationToken cancellationToken)
        => service.MapearBancoAsync(alias, banco);

    public Task<string> ExecutarConsultaAsync(string alias, string sql, string banco, int limite, CancellationToken cancellationToken)
        => service.ExecutarConsultaSomenteLeituraAsync(alias, sql, banco, limite);
}
