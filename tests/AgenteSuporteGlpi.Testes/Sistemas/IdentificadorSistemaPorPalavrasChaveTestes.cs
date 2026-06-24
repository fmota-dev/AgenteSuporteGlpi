using AgenteSuporteGlpi.Sistemas;
using FluentAssertions;

namespace AgenteSuporteGlpi.Testes.Sistemas;

public sealed class IdentificadorSistemaPorPalavrasChaveTestes
{
    private static readonly IReadOnlyList<SistemaConfigurado> SistemasPadrao =
    [
        new SistemaConfigurado
        {
            Nome = "Sistema de Pesquisas",
            Ativo = true,
            Aliases = ["pesquisas", "sistema de pesquisas"],
            PalavrasChave = ["pesquisa", "pergunta", "questionario"]
        },
        new SistemaConfigurado
        {
            Nome = "Sistema de Agenda",
            Ativo = true,
            Aliases = ["agenda", "sistema de agenda"],
            PalavrasChave = ["agenda", "agendamento", "compromisso", "evento"]
        }
    ];

    [Fact]
    public void Deve_identificar_sistema_por_alias_exato_no_titulo()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Sistema de Pesquisas - Criacao de novas perguntas",
            descricao: "Necessario criar novas perguntas no sistema.",
            sistemas: SistemasPadrao);

        resultado.Sistema.Should().NotBeNull();
        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_identificar_agenda_por_alias_no_titulo()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Agenda",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema.Should().NotBeNull();
        resultado.Sistema!.Nome.Should().Be("Sistema de Agenda");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_identificar_agenda_por_palavras_chave_na_descricao()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Responsavel nao recebe e-mails nem mensagens na agenda",
            descricao: "O responsavel nao recebe notificacoes de agendamento.",
            sistemas: SistemasPadrao);

        resultado.Sistema.Should().NotBeNull();
        resultado.Sistema!.Nome.Should().Be("Sistema de Agenda");
    }

    [Fact]
    public void Deve_identificar_pesquisas_por_palavra_chave_no_titulo()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Erro ao exportar pesquisa",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema.Should().NotBeNull();
        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
    }

    [Fact]
    public void Deve_pontuar_alias_no_titulo_mais_que_palavra_chave_na_descricao()
    {
        var sistemas = new List<SistemaConfigurado>
        {
            new() { Nome = "Sistema A", Ativo = true, Aliases = ["sistema a"], PalavrasChave = [] },
            new() { Nome = "Sistema B", Ativo = true, Aliases = [], PalavrasChave = ["sistema a"] }
        };

        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Problema no Sistema A",
            descricao: "sistema a com erro",
            sistemas: sistemas);

        resultado.Sistema!.Nome.Should().Be("Sistema A");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_retornar_nao_identificado_quando_nenhum_termo_encontrado()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Problema no Outlook",
            descricao: "Nao consigo enviar e-mails.",
            sistemas: SistemasPadrao);

        resultado.Confianca.Should().Be(NivelConfianca.NaoIdentificado);
        resultado.Sistema.Should().BeNull();
        resultado.TermosEncontrados.Should().BeEmpty();
    }

    [Fact]
    public void Deve_ignorar_sistema_inativo()
    {
        var sistemas = new List<SistemaConfigurado>
        {
            new() { Nome = "Inativo", Ativo = false, Aliases = ["sys-off"], PalavrasChave = [] },
        };

        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "sys-off",
            descricao: "",
            sistemas: sistemas);

        resultado.Confianca.Should().Be(NivelConfianca.NaoIdentificado);
    }

    [Fact]
    public void Deve_retornar_baixa_em_empate_entre_sistemas()
    {
        var sistemas = new List<SistemaConfigurado>
        {
            new() { Nome = "Sistema X", Ativo = true, Aliases = ["modulo x"], PalavrasChave = [] },
            new() { Nome = "Sistema Y", Ativo = true, Aliases = ["modulo y"], PalavrasChave = [] }
        };

        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "modulo x e modulo y",
            descricao: "",
            sistemas: sistemas);

        resultado.Confianca.Should().Be(NivelConfianca.Baixa);
        resultado.Sistema.Should().BeNull();
        resultado.Motivo.Should().Contain("Empate");
    }

    [Fact]
    public void Deve_normalizar_texto_sem_acentos_e_case_insensitive()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "SISTEMA DE PESQUISAS",
            descricao: "Questionário com erro",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_acumular_pontuacao_com_multiplos_termos_do_mesmo_sistema()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Pesquisas - nova pergunta",
            descricao: "questionario com erro",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
        resultado.Pontuacao.Should().BeGreaterThan(10);
    }

    [Fact]
    public void Deve_incluir_termos_encontrados_e_motivo_no_resultado()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Pesquisas",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.TermosEncontrados.Should().NotBeEmpty();
        resultado.Motivo.Should().NotBeNullOrWhiteSpace();
        resultado.Pontuacao.Should().BePositive();
    }

    [Fact]
    public void Deve_retornar_nao_identificado_com_lista_vazia_de_sistemas()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "qualquer coisa",
            descricao: "",
            sistemas: []);

        resultado.Confianca.Should().Be(NivelConfianca.NaoIdentificado);
    }

    [Fact]
    public void Deve_identificar_chamado_real_31905_sistema_pesquisas()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Sistema de pesquisas - Criacao de novas perguntas",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Pesquisas");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_identificar_chamado_real_31619_agenda()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Agenda",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Agenda");
        resultado.Confianca.Should().Be(NivelConfianca.Alta);
    }

    [Fact]
    public void Deve_identificar_chamado_real_31868_agenda_por_palavras_chave()
    {
        var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
            titulo: "Responsavel nao recebe e-mails nem mensagens na agenda",
            descricao: "",
            sistemas: SistemasPadrao);

        resultado.Sistema!.Nome.Should().Be("Sistema de Agenda");
    }
}
