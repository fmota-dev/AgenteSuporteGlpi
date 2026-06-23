namespace AgenteSuporteGlpi.Chamados;

public static class FiltroChamados
{
    private static readonly HashSet<StatusChamado> StatusPermitidos =
    [
        StatusChamado.Novo,
        StatusChamado.EmAtendimento,
        StatusChamado.Pendente
    ];

    public static IReadOnlyList<ChamadoColetado> FiltrarElegiveis(
        IEnumerable<ChamadoColetado> chamados,
        string responsavelConfigurado)
    {
        ArgumentNullException.ThrowIfNull(chamados);

        if (string.IsNullOrWhiteSpace(responsavelConfigurado))
        {
            throw new ArgumentException("Responsavel configurado e obrigatorio.", nameof(responsavelConfigurado));
        }

        return chamados
            .Where(chamado => StatusPermitidos.Contains(chamado.Status))
            .Where(chamado => string.Equals(
                chamado.Responsavel.Trim(),
                responsavelConfigurado.Trim(),
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(chamado => chamado.Numero)
            .ToArray();
    }
}
