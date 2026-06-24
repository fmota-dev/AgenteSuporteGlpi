using AgenteSuporteGlpi.Sistemas;

namespace AgenteSuporteGlpi.Chamados;

public interface IRepositorioChamados
{
    Task<string?> ObterUltimoHashAsync(int numeroChamado, CancellationToken cancellationToken);
    Task SalvarChamadoAsync(DetalhesChamadoColetado chamado, string hashConteudo, CancellationToken cancellationToken);
    Task PersistirIdentificacaoAsync(int numeroChamado, ResultadoIdentificacaoSistema resultado, CancellationToken cancellationToken);
}
