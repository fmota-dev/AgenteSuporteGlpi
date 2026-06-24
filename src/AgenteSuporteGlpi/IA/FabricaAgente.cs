using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AgenteSuporteGlpi.IA;

public static class FabricaAgente
{
    public static AIAgent Criar(ConfiguracaoIa configuracao)
    {
        return configuracao.Provider.ToLowerInvariant() switch
        {
            "ollama" => CriarOllama(configuracao),
            _ => throw new InvalidOperationException($"Provider '{configuracao.Provider}' nao implementado.")
        };
    }

    private static AIAgent CriarOllama(ConfiguracaoIa configuracao)
    {
        var endpoint = new Uri(configuracao.Endpoint);
        var credential = new ApiKeyCredential(configuracao.ApiKey ?? "ollama");

        var openAiClient = new OpenAIClient(credential, new OpenAIClientOptions
        {
            Endpoint = endpoint
        });

        var chatClient = openAiClient.GetChatClient(model: configuracao.Model);
        var aiChatClient = chatClient.AsIChatClient();

        return new ChatClientAgent(
            aiChatClient,
            instructions: configuracao.Instructions,
            name: "AnalisadorChamados",
            description: "Analisa chamados de suporte e produz resumo tecnico com perguntas e proximos passos.");
    }
}
