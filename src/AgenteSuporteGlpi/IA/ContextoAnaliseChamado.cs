using AgenteSuporteGlpi.Chamados;
using AgenteSuporteGlpi.Sistemas;

namespace AgenteSuporteGlpi.IA;

public sealed record ContextoAnaliseChamado
{
    public required DetalhesChamadoColetado Chamado { get; init; }
    public required ResultadoIdentificacaoSistema Identificacao { get; init; }
}
