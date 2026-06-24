using Microsoft.Extensions.Configuration;

namespace AgenteSuporteGlpi.Sistemas;

public static class ConfiguracaoSistemas
{
    public static IReadOnlyList<SistemaConfigurado> Carregar(IConfiguration configuration)
    {
        var secoes = configuration.GetSection("Sistemas").GetChildren();
        var sistemas = new List<SistemaConfigurado>();

        foreach (var secao in secoes)
        {
            var sistema = secao.Get<SistemaConfigurado>()
                ?? throw new InvalidOperationException(
                    $"Sistema na secao '{secao.Path}' nao pode ser desserializado. Verifique os campos obrigatorios.");

            if (string.IsNullOrWhiteSpace(sistema.Nome))
            {
                throw new InvalidOperationException(
                    $"Sistema na secao '{secao.Path}' requer o campo 'Nome'.");
            }

            sistemas.Add(sistema);
        }

        return sistemas;
    }
}
