using AgenteSuporteGlpi.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Chamados;

public sealed class FiltroChamadosTestes
{
    [Fact]
    public void Deve_manter_chamados_do_responsavel_com_status_permitido()
    {
        var chamados = new[]
        {
            NovoChamado(1, "Ana", StatusChamado.Novo),
            NovoChamado(2, "Bruno", StatusChamado.Novo),
            NovoChamado(3, "Ana", StatusChamado.EmAtendimento),
            NovoChamado(4, "Ana", StatusChamado.Solucionado)
        };

        var resultado = FiltroChamados.FiltrarElegiveis(chamados, "Ana");

        resultado.Select(chamado => chamado.Numero).Should().Equal(1, 3);
    }

    [Fact]
    public void Deve_tratar_responsavel_sem_diferenciar_maiusculas()
    {
        var chamados = new[]
        {
            NovoChamado(10, "ANA SILVA", StatusChamado.Pendente)
        };

        var resultado = FiltroChamados.FiltrarElegiveis(chamados, "ana silva");

        resultado.Should().ContainSingle().Which.Numero.Should().Be(10);
    }

    private static ChamadoColetado NovoChamado(int numero, string responsavel, StatusChamado status) =>
        new(
            numero,
            $"Chamado {numero}",
            status,
            "Media",
            responsavel,
            DateTimeOffset.Parse("2026-06-23T10:00:00-03:00"),
            new Uri($"https://glpi.local/front/ticket.form.php?id={numero}"));
}
