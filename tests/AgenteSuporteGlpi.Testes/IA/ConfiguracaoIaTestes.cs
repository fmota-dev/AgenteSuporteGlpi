using AgenteSuporteGlpi.IA;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace AgenteSuporteGlpi.Testes.IA;

public sealed class ConfiguracaoIaTestes
{
    [Fact]
    public void Deve_carregar_configuracao_com_valores_padrao()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AI:Provider", "ollama" },
                { "AI:Model", "llama3.2" }
            })
            .Build();

        var ia = ConfiguracaoIa.Carregar(config);

        ia.Provider.Should().Be("ollama");
        ia.Model.Should().Be("llama3.2");
        ia.Endpoint.Should().Be("http://localhost:11434/v1");
        ia.AgentMode.Should().Be("single");
        ia.TimeoutSegundos.Should().Be(45);
        ia.TimeoutAgente.Should().Be(TimeSpan.FromSeconds(45));
    }

    [Fact]
    public void Deve_lancar_para_provider_invalido()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AI:Provider", "invalido" },
                { "AI:Model", "llama3.2" }
            })
            .Build();

        var act = () => ConfiguracaoIa.Carregar(config);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Provider*invalido*");
    }

    [Fact]
    public void Deve_lancar_para_modelo_vazio()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AI:Provider", "ollama" },
                { "AI:Model", "" }
            })
            .Build();

        var act = () => ConfiguracaoIa.Carregar(config);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Model*");
    }

    [Fact]
    public void Deve_lancar_para_agent_mode_invalido()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AI:Provider", "ollama" },
                { "AI:Model", "llama3.2" },
                { "AI:AgentMode", "invalid" }
            })
            .Build();

        var act = () => ConfiguracaoIa.Carregar(config);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AgentMode*");
    }

    [Fact]
    public void Deve_corrigir_timeout_negativo_para_padrao()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AI:Provider", "ollama" },
                { "AI:Model", "llama3.2" },
                { "AI:TimeoutSegundos", "-5" }
            })
            .Build();

        var ia = ConfiguracaoIa.Carregar(config);

        ia.TimeoutSegundos.Should().Be(45);
    }

    [Fact]
    public void ProviderNome_deve_retornar_descricao_humana()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AI:Provider", "ollama" },
                { "AI:Model", "llama3.2" }
            })
            .Build();

        var ia = ConfiguracaoIa.Carregar(config);

        ia.ProviderNome.Should().Be("Ollama (OpenAI-compatible)");
    }
}
