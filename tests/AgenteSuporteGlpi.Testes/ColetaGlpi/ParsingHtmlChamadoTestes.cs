using AgenteSuporteGlpi.ColetaGlpi;
using AgenteSuporteGlpi.Chamados;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.ColetaGlpi;

public sealed class ParsingHtmlChamadoTestes
{
    [Fact]
    public async Task Deve_converter_html_de_detalhe_em_chamado()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-detalhe.html", TestContext.Current.CancellationToken);

        var chamado = ParserDetalhesChamado.Converter(
            html,
            new Uri("https://glpi.local/front/ticket.form.php?id=123"));

        chamado.Numero.Should().Be(123);
        chamado.Titulo.Should().Be("Erro ao salvar matricula");
        chamado.Status.Should().Be(StatusChamado.Novo);
        chamado.Descricao.Should().Be("Mensagem de erro ao salvar matricula do aluno.");
        chamado.Responsavel.Should().Be("Ana");
    }
}
