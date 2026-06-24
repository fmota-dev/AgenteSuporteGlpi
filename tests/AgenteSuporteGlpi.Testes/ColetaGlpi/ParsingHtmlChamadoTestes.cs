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
        chamado.Prioridade.Should().Be("Media");
        chamado.Categoria.Should().Be("Academico");
        chamado.Solicitante.Should().Be("Maria");
        chamado.Responsavel.Should().Be("Ana");
        chamado.DataAbertura.Should().Be(DateTimeOffset.Parse("2026-06-23T09:00:00-03:00"));
        chamado.DataUltimaAtualizacao.Should().Be(DateTimeOffset.Parse("2026-06-23T10:00:00-03:00"));
        chamado.Descricao.Should().Be("Mensagem de erro ao salvar matricula do aluno.");
        chamado.Link.Should().Be(new Uri("https://glpi.local/front/ticket.form.php?id=123"));
    }

    [Fact]
    public async Task Deve_remover_tags_html_aninhadas_nos_valores()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-tags-aninhadas.html", TestContext.Current.CancellationToken);

        var chamado = ParserDetalhesChamado.Converter(
            html,
            new Uri("https://glpi.local/front/ticket.form.php?id=456"));

        chamado.Titulo.Should().Be("Erro ao salvar matricula");
        chamado.Categoria.Should().Be("Academico");
        chamado.Responsavel.Should().Be("Ana");
        chamado.Descricao.Should().Be("Mensagem de erro ao salvar matricula do aluno.");
    }

    [Fact]
    public async Task Deve_retornar_null_para_campos_opcionais_ausentes()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-sem-opcionais.html", TestContext.Current.CancellationToken);

        var chamado = ParserDetalhesChamado.Converter(
            html,
            new Uri("https://glpi.local/front/ticket.form.php?id=789"));

        chamado.Categoria.Should().BeNull();
        chamado.Solicitante.Should().BeNull();
        chamado.Numero.Should().Be(789);
        chamado.Responsavel.Should().Be("Jose");
    }

    [Fact]
    public async Task Deve_lancar_excecao_para_campo_obrigatorio_ausente()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-sem-campo-obrigatorio.html", TestContext.Current.CancellationToken);

        var acao = () => ParserDetalhesChamado.Converter(
            html,
            new Uri("https://glpi.local/front/ticket.form.php?id=999"));

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*Numero*");
    }

    [Fact]
    public async Task Deve_converter_status_em_atendimento()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-status-em-atendimento.html", TestContext.Current.CancellationToken);

        var chamado = ParserDetalhesChamado.Converter(
            html,
            new Uri("https://glpi.local/front/ticket.form.php?id=200"));

        chamado.Status.Should().Be(StatusChamado.EmAtendimento);
        chamado.Numero.Should().Be(200);
    }

    [Fact]
    public async Task Deve_converter_status_solucionado()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-status-solucionado.html", TestContext.Current.CancellationToken);

        var chamado = ParserDetalhesChamado.Converter(
            html,
            new Uri("https://glpi.local/front/ticket.form.php?id=300"));

        chamado.Status.Should().Be(StatusChamado.Solucionado);
        chamado.Numero.Should().Be(300);
    }

    [Fact]
    public async Task Deve_converter_html_glpi10_com_timeline_e_fallback_da_lista()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-detalhe-glpi10.html", TestContext.Current.CancellationToken);

        var chamadoLista = new ChamadoColetado(
            789,
            "Erro ao salvar matricula (789)",
            StatusChamado.Novo,
            "Média",
            "Ana",
            DateTimeOffset.Parse("2026-06-23T10:00:00-03:00"),
            new Uri("https://glpi.local/front/ticket.form.php?id=789"));

        var chamado = ParserDetalhesChamado.Converter(html, chamadoLista);

        chamado.Numero.Should().Be(789);
        chamado.Titulo.Should().Be("Erro ao salvar matricula");
        chamado.Status.Should().Be(StatusChamado.Novo);
        chamado.Prioridade.Should().Be("Média");
        chamado.Categoria.Should().Be("Academico");
        chamado.Solicitante.Should().Be("Maria");
        chamado.Responsavel.Should().Be("Ana");
        chamado.Descricao.Should().Contain("Mensagem de erro ao salvar matricula do aluno");
    }

    [Fact]
    public async Task Deve_usar_fallback_da_lista_quando_html_nao_tem_metadados()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-detalhe-glpi10-real.html", TestContext.Current.CancellationToken);

        var chamadoLista = new ChamadoColetado(
            31619,
            "Agenda (31619)",
            StatusChamado.Pendente,
            "Alta",
            "Filipe Mota",
            DateTimeOffset.UtcNow,
            new Uri("https://glpi.local/front/ticket.form.php?id=31619"));

        var detalhes = ParserDetalhesChamado.Converter(html, chamadoLista);

        detalhes.Numero.Should().Be(31619);
        detalhes.Titulo.Should().Be("Agenda");
        detalhes.Status.Should().Be(StatusChamado.Pendente);
        detalhes.Prioridade.Should().Be("Alta");
        detalhes.Responsavel.Should().Be("Filipe Mota");
        detalhes.Descricao.Should().Contain("Sistema de Agenda");
        detalhes.Descricao.Should().Contain("falha na sincronizacao");
    }

    [Fact]
    public async Task Deve_extrair_titulo_de_h3_navigationheader_title()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-detalhe-glpi10.html", TestContext.Current.CancellationToken);

        var chamadoLista = new ChamadoColetado(
            789, "Erro ao salvar matricula (789)", StatusChamado.Novo, "Média", "Ana",
            DateTimeOffset.UtcNow, new Uri("https://glpi.local/front/ticket.form.php?id=789"));

        var chamado = ParserDetalhesChamado.Converter(html, chamadoLista);

        chamado.Titulo.Should().Be("Erro ao salvar matricula");
        chamado.Titulo.Should().NotContain("(789)");
    }

    [Fact]
    public async Task Deve_extrair_status_de_aria_label_no_header()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-detalhe-glpi10.html", TestContext.Current.CancellationToken);

        var chamadoLista = new ChamadoColetado(
            789, "Erro ao salvar matricula (789)", StatusChamado.Novo, "Média", "Ana",
            DateTimeOffset.UtcNow, new Uri("https://glpi.local/front/ticket.form.php?id=789"));

        var chamado = ParserDetalhesChamado.Converter(html, chamadoLista);

        chamado.Status.Should().Be(StatusChamado.Novo);
    }

    [Fact]
    public async Task Deve_extrair_status_de_data_bs_original_title_no_header()
    {
        var html = await File.ReadAllTextAsync("Fixtures/chamado-detalhe-glpi10-real.html", TestContext.Current.CancellationToken);

        var chamadoLista = new ChamadoColetado(
            31619, "Agenda (31619)", StatusChamado.Novo, "Média", "Filipe Mota",
            DateTimeOffset.UtcNow, new Uri("https://glpi.local/front/ticket.form.php?id=31619"));

        var detalhes = ParserDetalhesChamado.Converter(html, chamadoLista);

        detalhes.Status.Should().Be(StatusChamado.Pendente);
    }
}
