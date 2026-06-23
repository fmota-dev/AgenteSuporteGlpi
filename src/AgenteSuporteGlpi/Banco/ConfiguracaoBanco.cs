namespace AgenteSuporteGlpi.Banco;

public sealed class ConfiguracaoBanco
{
    public string ConnectionString { get; init; } = "Data Source=dados/agente-suporte-glpi.db";
}
