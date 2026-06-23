namespace AgenteSuporteGlpi.Configuracao;

public sealed class ConfiguracaoBrowser
{
    public bool Headless { get; init; } = true;
    public int TimeoutMilissegundos { get; init; } = 30_000;
    public int TimeoutEsperaAjaxMilissegundos { get; init; } = 5_000;
}
