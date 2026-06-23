using AgenteSuporteGlpi.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Chamados;

public sealed class ResultadoMudancaChamadoTestes
{
    [Fact]
    public void Deve_marcar_como_novo_quando_nao_houver_hash_anterior()
    {
        var resultado = ResultadoMudancaChamado.Avaliar(hashAnterior: null, hashAtual: "abc");

        resultado.EhNovo.Should().BeTrue();
        resultado.FoiAlterado.Should().BeTrue();
    }

    [Fact]
    public void Deve_marcar_sem_alteracao_quando_hash_for_igual()
    {
        var resultado = ResultadoMudancaChamado.Avaliar("abc", "abc");

        resultado.EhNovo.Should().BeFalse();
        resultado.FoiAlterado.Should().BeFalse();
    }

    [Fact]
    public void Deve_marcar_alterado_quando_hash_mudar()
    {
        var resultado = ResultadoMudancaChamado.Avaliar("abc", "def");

        resultado.EhNovo.Should().BeFalse();
        resultado.FoiAlterado.Should().BeTrue();
    }
}
