using AgenteSuporteGlpi.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Chamados;

public sealed class HashConteudoChamadoTestes
{
    [Fact]
    public void Deve_gerar_mesmo_hash_para_texto_com_espacos_equivalentes()
    {
        var primeiro = HashConteudoChamado.Calcular("Erro ao salvar   matricula");
        var segundo = HashConteudoChamado.Calcular(" Erro ao salvar matricula ");

        primeiro.Should().Be(segundo);
    }

    [Fact]
    public void Deve_gerar_hash_diferente_quando_conteudo_mudar()
    {
        var primeiro = HashConteudoChamado.Calcular("Erro ao salvar matricula");
        var segundo = HashConteudoChamado.Calcular("Erro ao excluir matricula");

        primeiro.Should().NotBe(segundo);
    }
}
