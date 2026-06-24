using Microsoft.Extensions.Configuration;

namespace AgenteSuporteGlpi.IA;

public sealed record ConfiguracaoIa
{
    public string Provider { get; init; } = "ollama";
    public string Endpoint { get; init; } = "http://localhost:11434/v1";
    public string Model { get; init; } = "llama3.2";
    public string? ApiKey { get; init; }
    public string Instructions { get; init; } = "Voce e um assistente tecnico especializado em analise de chamados de suporte.";
    public string AgentMode { get; init; } = "single";
    public int TimeoutSegundos { get; init; } = 45;

    public TimeSpan TimeoutAgente => TimeSpan.FromSeconds(TimeoutSegundos);

    public string ProviderNome => Provider switch
    {
        "ollama" => "Ollama (OpenAI-compatible)",
        "openrouter" => "OpenRouter",
        "github" => "GitHub Models",
        _ => Provider
    };

    public static ConfiguracaoIa Carregar(IConfiguration configuration)
    {
        var config = new ConfiguracaoIa();
        configuration.GetSection("AI").Bind(config);

        var provider = config.Provider.ToLowerInvariant();
        var providersValidos = new[] { "ollama", "openrouter", "github", "gemini", "anthropic" };

        if (!providersValidos.Contains(provider))
        {
            throw new InvalidOperationException(
                $"Provider '{config.Provider}' invalido. Valores aceitos: {string.Join(", ", providersValidos)}.");
        }

        if (config.AgentMode is not "single" and not "multi")
        {
            throw new InvalidOperationException(
                $"AgentMode '{config.AgentMode}' invalido. Use 'single' ou 'multi'.");
        }

        if (string.IsNullOrWhiteSpace(config.Model))
        {
            throw new InvalidOperationException("AI:Model obrigatorio.");
        }

        if (config.TimeoutSegundos <= 0)
        {
            config = config with { TimeoutSegundos = 45 };
        }

        return config;
    }
}
