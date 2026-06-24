using AgenteSuporteGlpi.Contratos;

namespace AgenteSuporteGlpi.SqlServer;

public sealed class ConsultaSqlServerIndisponivel : IConsultaSqlServerSomenteLeitura
{
    public Task<string> ListarConexoesAsync(CancellationToken cancellationToken)
        => Task.FromResult("{}");

    public Task<string> ListarBancosAsync(string alias, CancellationToken cancellationToken)
        => Task.FromResult("{}");

    public Task<string> MapearBancoAsync(string alias, string banco, CancellationToken cancellationToken)
        => Task.FromResult("{}");

    public Task<string> ExecutarConsultaAsync(string alias, string sql, string banco, int limite, CancellationToken cancellationToken)
        => Task.FromResult("{}");
}
