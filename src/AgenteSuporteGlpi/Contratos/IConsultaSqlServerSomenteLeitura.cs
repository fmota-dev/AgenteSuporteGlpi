namespace AgenteSuporteGlpi.Contratos;

public interface IConsultaSqlServerSomenteLeitura
{
    Task<string> ListarBancosAsync(string alias, CancellationToken cancellationToken);
    Task<string> MapearBancoAsync(string alias, string banco, CancellationToken cancellationToken);
    Task<string> ExecutarConsultaAsync(string alias, string sql, string banco, int limite, CancellationToken cancellationToken);
}
