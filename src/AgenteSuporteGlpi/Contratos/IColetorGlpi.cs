using AgenteSuporteGlpi.Chamados;

namespace AgenteSuporteGlpi.Contratos;

public interface IColetorGlpi
{
    Task<IReadOnlyList<ChamadoColetado>> ColetarListaAsync(CancellationToken cancellationToken);
    Task<DetalhesChamadoColetado> ColetarDetalhesAsync(ChamadoColetado chamado, CancellationToken cancellationToken);
}
