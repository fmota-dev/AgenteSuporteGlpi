using AgenteSuporteGlpi.IA;
using AgenteSuporteGlpi.Sistemas;

namespace AgenteSuporteGlpi.Chamados;

public interface IRepositorioChamados
{
    Task<string?> ObterUltimoHashAsync(int numeroChamado, CancellationToken cancellationToken);
    Task SalvarChamadoAsync(DetalhesChamadoColetado chamado, string hashConteudo, CancellationToken cancellationToken);
    Task PersistirIdentificacaoAsync(int numeroChamado, ResultadoIdentificacaoSistema resultado, CancellationToken cancellationToken);
    Task<IReadOnlyList<DetalhesChamadoColetado>> ObterChamadosNaoAnalisadosAsync(CancellationToken cancellationToken);
    Task SalvarAnaliseIaAsync(int numeroChamado, ResultadoAnaliseIa analise, CancellationToken cancellationToken);
    Task MarcarAnalisadoPorIaAsync(int numeroChamado, CancellationToken cancellationToken);
}
